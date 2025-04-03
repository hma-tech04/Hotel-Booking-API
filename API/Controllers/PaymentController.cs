using System.Security.Claims;
using API.DTOs.EntityDTOs;
using API.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly BookingService _bookingService;

    public PaymentController(PaymentService paymentService, BookingService bookingService)
    {
        _paymentService = paymentService;
        _bookingService = bookingService;
    }
    
    // Get payment status of a room
    [Authorize(Roles = "User,Admin")]
    [HttpGet("booking/{bookingId}")]
    public async Task<IActionResult> GetPaymentStatusByBookingId(int bookingId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        var paymentStatus = await _paymentService.GetPaymentStatusByBookingId(bookingId);

        if (role == "Admin")
        {
            return Ok(new ApiResponse<List<PaymentDTO>>(ErrorCode.OK, "Success", paymentStatus));
        }

        var booking = await _bookingService.GetBookingByIdAsync(bookingId);
        if (booking == null || booking.UserId.ToString() != userId)
        {
            throw new CustomException(ErrorCode.Forbidden, "You do not have permission to view this booking.");
        }

        return Ok(new ApiResponse<List<PaymentDTO>>(ErrorCode.OK, "Success", paymentStatus));
    }

}