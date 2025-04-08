using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.DTOs.EntityDTOs;
using API.DTOs.PaymentDTO;
using API.Enum;
using API.Models;


public class VNPayService
{
    private readonly IConfiguration _configuration;
    private readonly string _vnpUrl;
    private readonly string _vnpTmnCode;
    private readonly string _vnpHashSecret;
    private readonly string _vnpReturnUrl;
    private readonly IBookingRepository _bookingService;
    private readonly PaymentService _paymentService;
    private readonly IRoomRepository _roomService;
    private readonly string _timeZoneID;

    public VNPayService(IRoomRepository roomService, IConfiguration configuration, IBookingRepository bookingRepository, PaymentService paymentService)
    {
        _roomService = roomService;
        _configuration = configuration;
        _bookingService = bookingRepository;
        _vnpUrl = _configuration["VNPay:BaseUrl"] ?? throw new InvalidOperationException("Missing VNPay BaseUrl.");
        _vnpTmnCode = _configuration["VNPay:TmnCode"] ?? throw new InvalidOperationException("Missing VNPay TmnCode.");
        _vnpHashSecret = _configuration["VNPay:HashSecret"] ?? throw new InvalidOperationException("Missing VNPay HashSecret.");
        _vnpReturnUrl = _configuration["VNPay:ReturnUrl"] ?? throw new InvalidOperationException("Missing VNPay ReturnUrl.");
        _timeZoneID = _configuration["TimeZoneID"] ?? throw new InvalidOperationException("Missing TimeZoneID.");
        _paymentService = paymentService;
    }

    public async Task<string> CreatePaymentUrlAsync(PaymentRequest request, HttpContext httpContext)
    {
        if (request == null || request.Amount <= 0 || string.IsNullOrEmpty(request.OrderId))
        {
            throw new ArgumentException("Invalid payment request");
        }
        if(await _paymentService.CheckPaymentStatusOfBooking(int.Parse(request.OrderId))){
            throw new CustomException(ErrorCode.BadRequest, "This order has already been paid.");
        }
        PaymentDTO paymentDTO = new PaymentDTO
        {
            BookingId = int.Parse(request.OrderId),
            PaymentAmount = request.Amount
        };

        var payment = await _paymentService.CreatePaymentAsync(paymentDTO);

        var booking = await _bookingService.GetBookingByIdAsync(int.Parse(request.OrderId));
        if(booking == null){
            throw new CustomException(ErrorCode.NotFound, $"Booking not found with id {request.OrderId}");
        }
        booking.PaymentId =payment.PaymentId;

        await _bookingService.UpdateBookingAsync(booking);

        // Calculate amout price
        decimal Price = booking.Room.Price;
        int NumberDay = (booking.CheckOutDate - booking.CheckInDate).Days;
        decimal TotalPrice = NumberDay * Price;
        Console.WriteLine("Total Price: " + TotalPrice);
        // check amount
        if(request.Amount != TotalPrice){
            throw new CustomException(ErrorCode.InvalidPaymentAmount, "The payment amount does not match the booking amount.");
        }
        
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(_timeZoneID);
        var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

        var pay = new SortedDictionary<string, string>
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", _vnpTmnCode },
            { "vnp_IpAddr", ipAddress },
            { "vnp_Amount", (request.Amount * 100).ToString() },
            { "vnp_CurrCode", "VND" },
            { "vnp_TxnRef", payment.PaymentId.ToString() },
            { "vnp_OrderInfo", JsonSerializer.Serialize(new { OrderId = request.OrderId, PaymentId = payment.PaymentId }) },
            { "vnp_OrderType", "billpayment" },
            { "vnp_Locale", "vn" },
            { "vnp_ReturnUrl", _vnpReturnUrl },
            { "vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss") }
        };

        string queryString = string.Join("&", pay.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        string secureHash = HmacSha512(_vnpHashSecret, queryString);
        pay["vnp_SecureHash"] = secureHash;

        return await Task.FromResult($"{_vnpUrl}?{queryString}&vnp_SecureHash={secureHash}");
    }

    public async Task<PaymentResponse> ProcessPaymentResponse(Dictionary<string, string> queryParams)
    {
        if (queryParams == null || !queryParams.ContainsKey("vnp_SecureHash"))
            return new PaymentResponse { IsSuccess = false, Message = "Invalid response data" };

        string receivedHash = queryParams["vnp_SecureHash"];
        queryParams.Remove("vnp_SecureHash");
        queryParams.Remove("vnp_SecureHashType");

        bool isValid = ValidateSignature(queryParams, receivedHash);
        queryParams.TryGetValue("vnp_ResponseCode", out string? responseCode);

        string paymentMessage = responseCode == "00" ? "Payment successful" : "Payment failed";
        bool isPaymentSuccessful = isValid && responseCode == "00";

        var status = isPaymentSuccessful ? BookingStatus.Confirmed : BookingStatus.Cancelled;
        var statusPayment = isPaymentSuccessful ? PaymentStatus.Completed : PaymentStatus.Failed;
        string orderInfoJson = queryParams["vnp_OrderInfo"];
        using JsonDocument doc = JsonDocument.Parse(orderInfoJson);
        int paymentId = doc.RootElement.GetProperty("PaymentId").GetInt32();
        int bookingIdInt = int.Parse(doc.RootElement.GetProperty("OrderId").GetString() ?? string.Empty);
        var isUpdatedPaymentStatus = await _paymentService.UpdatePaymentStatus(paymentId, statusPayment);
        if (!isPaymentSuccessful){
            Booking booking = await _bookingService.GetBookingByIdAsync(bookingIdInt) ?? throw new CustomException(ErrorCode.NotFound, $"Booking not found with id {bookingIdInt}");
            Room room = await _roomService.GetRoomByIDAsync(booking.RoomId) ?? throw new CustomException(ErrorCode.NotFound, $"Room not found with id {booking.RoomId}");
            room.IsAvailable = true;
            await _roomService.UpdateRoomAsync(room);
        }
        var isUpdated = await _bookingService.UpdateBookingStatusAsync(bookingIdInt, status);

        return new PaymentResponse { IsSuccess = isUpdated && isUpdatedPaymentStatus && isPaymentSuccessful, Message = isUpdated ? paymentMessage : "Failed to update booking status" };
    }

    private bool ValidateSignature(Dictionary<string, string> queryParams, string receivedHash)
    {
        string queryString = string.Join("&", queryParams.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        return HmacSha512(_vnpHashSecret, queryString).Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
    }

    private string HmacSha512(string key, string inputData)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        return string.Concat(hmac.ComputeHash(Encoding.UTF8.GetBytes(inputData)).Select(b => b.ToString("x2")));
    }
}
