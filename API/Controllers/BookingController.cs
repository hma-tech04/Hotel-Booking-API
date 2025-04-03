using API.DTOs.EntityDTOs;
using API.DTOs.Request;
using API.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingController : ControllerBase
{
    private readonly BookingService _bookingService;

    public BookingController(BookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // Create a new booking
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> AddBookingAsync(BookingRequest request)
    {
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _bookingService.AddBookingAsync(request);
        var response = new ApiResponse<BookingDTO>(ErrorCode.OK, "Booking created successfully", result);
        return Ok(response);
    }

    // Get user bookings by user ID
    [Authorize]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetBookingsByUserId(int userId)
    {
        var result = await _bookingService.GetBookingsByUserIdAsync(userId);
        ApiResponse<List<BookingDTO>> response = new ApiResponse<List<BookingDTO>>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }

    // Get booking by bookingID
    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookingById(int id)
    {
        var result = await _bookingService.GetBookingByIdAsync(id);
        var response = new ApiResponse<BookingDTO>(ErrorCode.OK, "Success", result);
        return Ok(response.GetResponse());
    }

    [Authorize(Roles = "Admin")]
    [HttpGet()]
    public async Task<IActionResult> GetAllBooking()
    {
        var result = await _bookingService.GetAllBookingAsync();
        ApiResponse<List<BookingDTO>> response = new ApiResponse<List<BookingDTO>>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }

    // Cancel a booking by bookingID
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        bool success = await _bookingService.CancelBookingAsync(id);
        ApiResponse<Boolean> response;
        if (!success)
        {
            response = new ApiResponse<Boolean>(ErrorCode.BadRequest, "Cancel booking failed", success);
            return NotFound(response);
        }
        response = new ApiResponse<Boolean>(ErrorCode.OK, "Booking cancelled successfully", success);

        return Ok(response);
    }

    // Check-in for a booking
    [Authorize(Roles = "Admin")]
    [HttpPost("check-in/{id}")]
    public async Task<IActionResult> CheckInBooking(int id)
    {
        bool success = await _bookingService.CheckInBooking(id);
        ApiResponse<bool> response;
        if (!success)
        {
            response = new ApiResponse<bool>(ErrorCode.BadRequest, "Check in booking failed", success);
            return NotFound(response);
        }
        response = new ApiResponse<bool>(ErrorCode.OK, "Check in successfully", success);

        return Ok(response);
    }

    // Check-out for a booking
    [Authorize(Roles = "Admin")]
    [HttpPost("check-out/{id}")]
    public async Task<IActionResult> CheckOutBooking(int id)
    {
        bool success = await _bookingService.CheckOutBooking(id);
        ApiResponse<bool> response;
        if (!success)
        {
            response = new ApiResponse<bool>(ErrorCode.OK, "Check out booking failed", success);
            return NotFound(response);
        }
        response = new ApiResponse<bool>(ErrorCode.OK, "Check out successfully", success);

        return Ok(response);
    }
}