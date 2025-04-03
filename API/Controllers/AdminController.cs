using API.DTOs.EntityDTOs;
using API.DTOs.Request;
using API.DTOs.Response;
using API.DTOs.Statistics;
using DTOs.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly UserAdminService _userAdminService;
    private readonly BookingService _bookingService;
    private readonly PaymentService _paymentService;
    public AdminController(UserAdminService userAdminService, BookingService bookingService, PaymentService paymentService)
    {
        _userAdminService = userAdminService;
        _bookingService = bookingService;
        _paymentService = paymentService;
    }

    // Get all users
    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllAdminAsync()
    {
        var result = await _userAdminService.GetAllUsersAsync();
        ApiResponse<IEnumerable<UserDTO>> response = new ApiResponse<IEnumerable<UserDTO>>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }


    // Delete user by ID
    [Authorize(Roles = "Admin")]
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUserByIDAsync(int id)
    {
        var result = await _userAdminService.DeleteUserAsync(id);
        ApiResponse<UserDTO> response = new ApiResponse<UserDTO>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }

    // Update role user
    [Authorize(Roles = "Admin")]
    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateRoleUserAsync(int id, UpdateUserRoleDTO updateUserRoleDTO)
    {
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _userAdminService.UpdateRoleUserAsync(id, updateUserRoleDTO);
        ApiResponse<UserDTO> response = new ApiResponse<UserDTO>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }   

    // Get booking statistics within a specified time range
    [Authorize(Roles = "Admin")]
    [HttpGet("statistics/bookings")]
    public async Task<IActionResult> RetrieveBookingStats(DateRangeDto dateRangeDto)
    {
        var result = await _bookingService.RetrieveBookingStats(dateRangeDto);
        ApiResponse<List<BookingDTO>> response = new ApiResponse<List<BookingDTO>>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }

    // Get revenue statistics by month
    [Authorize(Roles = "Admin")]
    [HttpGet("statistics/revenue/month")]
    public async Task<IActionResult> GetRevenueMonth(MonthlyRevenueRequestDto requestDto)
    {
        var revenue = await _paymentService.RetrieveMonthlyRevenue(requestDto);
        ApiResponse<RevenueResponse> response = new ApiResponse<RevenueResponse>(ErrorCode.OK, "Success", revenue);
        return Ok(response);
    }

    // Get total revenue statistics
    [Authorize(Roles = "Admin")]
    [HttpGet("statistics/revenue")]
    public async Task<IActionResult> GetTotalRevenue()
    {
        var revenue = await _paymentService.RetrieveToltalRevenue();
        ApiResponse<RevenueResponse> response = new ApiResponse<RevenueResponse>(ErrorCode.OK, "Success", revenue);
        return Ok(response);
    }

    // Check rooms not checked-in by phone number
    [Authorize(Roles = "Admin")]
    [HttpPost("bookings/unchecked")]
    public async Task<IActionResult> GetUncheckedBookingsByPhoneNumber(PhoneNumberRequestDto requestDto)
    {
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.InvalidData, "Phone number must be between 10 and 15 digits.");
        }
        var result = await _bookingService.GetUncheckedInBookingsByPhoneNumber(requestDto);
        ApiResponse<List<BookingDTO>> response = new ApiResponse<List<BookingDTO>>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }
}