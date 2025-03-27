using API.DTOs;
using API.DTOs.EntityDTOs;
using API.DTOs.Request;
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
        public async Task<IActionResult> AddBookingAsync(BookingRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new CustomException(ErrorCode.BadRequest, "Invalid input");
            }
            var result = await _bookingService.AddBookingAsync(request);
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
            var response = new ApiResponse<BookingDTO>(200, "Success", result);
            return Ok(response.GetResponse());
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            bool success = await _bookingService.CancelBookingAsync(id);
            ApiResponse<Boolean> response;
            if (!success)
            {
                response = new ApiResponse<Boolean>(400, "Cancel booking failed", success);
                return NotFound(response);
            }
            response = new ApiResponse<Boolean>(200, "Booking cancelled successfully", success);

            return Ok(response);
        }
    }
}