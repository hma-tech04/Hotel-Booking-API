using Microsoft.AspNetCore.Mvc;
using API.DTOs.Response;
using API.DTOs.PaymentDTO;
using DTOs.Signature;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
public class VNPayController : ControllerBase
{
    private readonly VNPayService _vnpayService;
    private readonly SignatureService _signatureService;

    public VNPayController(VNPayService vnpayService, SignatureService signatureService)
    {
        _vnpayService = vnpayService;
        _signatureService = signatureService;
    }

    // Create url payment
    [Authorize]
    [HttpPost("create-payment")]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest paymentRequest)
    {
        if (paymentRequest.Amount < 10000)
        {
            throw new CustomException(ErrorCode.BadRequest, "Amount must be greater than 1000");
        }

        var result = await _vnpayService.CreatePaymentUrlAsync(paymentRequest, HttpContext);

        return Ok(new ApiResponse<string>(ErrorCode.OK, "Success", result));
    }

    // Call back payment for VNPAY
    [Authorize]
    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        Dictionary<string, string> vnp_Params = Request.Query
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
        var result = await _vnpayService.ProcessPaymentResponse(vnp_Params);
        string paymentStatus = result.IsSuccess ? "Payment successfully" : "Payment Failed";
        string signature = _signatureService.GenerateSignature(paymentStatus);

        return Redirect($"http://localhost:3000/payment-result?paymentStatus={paymentStatus}&signature={signature}");
    }

    // Verify signature paymentStatus after payment successfuly
    [Authorize]
    [HttpPost("verify-signature")]
    public async Task<IActionResult> VerifySignature([FromBody] SignatureRequest signatureRequest)
    {
        signatureRequest.Signature = signatureRequest.Signature.Replace(" ", "+");
        var isValid = _signatureService.VerifySignature(signatureRequest.data, signatureRequest.Signature);
        if (!isValid)
        {
            throw new CustomException(ErrorCode.Unauthorized, "Invalid signature");
        }
        return await Task.FromResult(Ok(new ApiResponse<bool>(ErrorCode.OK, "Valid signature", true)));
    }
}