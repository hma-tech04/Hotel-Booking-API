using API.DTOs.EntityDTOs;
using API.DTOs.Statistics;
using API.Enum;
using API.Models;
using AutoMapper;

public class PaymentService{

    private readonly IPaymentRepository _paymentRepository;
    private readonly IBookingRepository _bookingService;
    private readonly IMapper _mapper;

    public PaymentService(IPaymentRepository paymentRepository, IBookingRepository bookingService, IMapper mapper){
        _paymentRepository = paymentRepository;
        _bookingService = bookingService;
        _mapper = mapper;
    }

    public async Task<PaymentDTO> CreatePaymentAsync(PaymentDTO paymentDTO){
        await _bookingService.GetBookingByIdAsync(paymentDTO.BookingId);
        Payment payment = _mapper.Map<Payment>(paymentDTO);
        var paymentCreated = await _paymentRepository.CreatePayment(payment);
        return _mapper.Map<PaymentDTO>(paymentCreated);
    }

    public async Task<PaymentDTO> GetPaymentById(int id){
        var payment = await _paymentRepository.GetPaymentById(id);

        if(payment == null){
            throw new CustomException(ErrorCode.NotFound, $"Payment not found with {id}");
        }
        return _mapper.Map<PaymentDTO>(payment);
    }

    public async Task<List<PaymentDTO>> GetPaymentStatusByBookingId(int bookingId){
        await _bookingService.GetBookingByIdAsync(bookingId);
        var payments = await _paymentRepository.GetPaymentsByBookingId(bookingId);
        return _mapper.Map<List<PaymentDTO>>(payments);
    }

    public async Task<bool> UpdatePaymentStatus(int paymentId, PaymentStatus paymentStatus){
        return await _paymentRepository.UpdatePaymentStatus(paymentId, paymentStatus);
    }

    public async Task<bool> CheckPaymentStatusOfBooking(int bookingId){
        var list = await GetPaymentStatusByBookingId(bookingId);

        foreach(var item in list){
            if(item.PaymentStatus == PaymentStatus.Completed)
                return true;
        }
        return false;
    }

    public async Task<RevenueResponse> RetrieveMonthlyRevenue(MonthlyRevenueRequestDto monthlyRevenue){
        var Revenue = await _paymentRepository.RetrieveMonthlyRevenue(monthlyRevenue);
        return new RevenueResponse(){
            Revenue = Revenue
        };
    }

    public async Task<RevenueResponse> RetrieveToltalRevenue(){
        var Revenue = await _paymentRepository.RetrieveToltalRevenue();
        return new RevenueResponse(){
            Revenue = Revenue
        };
    }
}