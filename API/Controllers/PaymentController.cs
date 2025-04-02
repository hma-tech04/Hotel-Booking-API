using API.DTOs.EntityDTOs;
using API.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase{
    private readonly PaymentService _paymentService;

    public PaymentController(PaymentService paymentService){
        _paymentService = paymentService;
    }

    [HttpGet("booking/{bookingId}")]
    public async Task<IActionResult> GetPaymentStatusByBookingId(int bookingId){
        var result = await _paymentService.GetPaymentStatusByBookingId(bookingId);
        ApiResponse<List<PaymentDTO>> response = new ApiResponse<List<PaymentDTO>>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }
}