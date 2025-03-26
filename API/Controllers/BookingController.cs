using API.DTOs;
using API.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _bookingService;

        public BookingController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> AddBookingAsync([FromBody] BookingDTO bookingDTO)
        {
            var result = await _bookingService.AddBookingAsync(bookingDTO);
            var response = new ApiResponse<BookingDTO>(201, "Booking created successfully", result);
            return Ok(response);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBookingsByUserId(int userId)
        {
            var result = await _bookingService.GetBookingsByUserIdAsync(userId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            var result = await _bookingService.GetBookingByIdAsync(id);
            if (result == null) return NotFound("Booking not found");

            return Ok(result);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            bool success = await _bookingService.CancelBookingAsync(id);
            if (!success) return NotFound("Booking not found or already cancelled");

            return Ok(new { message = "Booking cancelled successfully" });
        }
    }
}
