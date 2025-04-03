using Microsoft.AspNetCore.Mvc;
using API.DTOs.Response;
using API.Services;
using API.DTOs.EntityDTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    // Get user by id
    [Authorize]
    [HttpGet("id/{id}")]
    public async Task<IActionResult> GetUserByIDAsync(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            throw new CustomException(ErrorCode.Unauthorized, "User not authenticated.");
        }

        if (id != int.Parse(userId))
        {
            throw new CustomException(ErrorCode.Forbidden, "You do not have permission to access this user data.");
        }
        var result = await _userService.GetUserByIdAsync(id);
        ApiResponse<UserDTO> response = new ApiResponse<UserDTO>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }

    // Get user by email
    [Authorize]
    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetUserByEmailAsync(string email)
    {
        var emailOfUser = User.FindFirst(ClaimTypes.Email)?.Value;
        if (emailOfUser == null)
        {
            throw new CustomException(ErrorCode.Unauthorized, "User not authenticated.");
        }
        if (email != emailOfUser.ToString())
        {
            throw new CustomException(ErrorCode.Forbidden, "You do not have permission to access this user data.");
        }
        var result = await _userService.GetUserByEmailAsync(email);
        ApiResponse<UserDTO> response = new ApiResponse<UserDTO>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }

    // Update user
    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateUserAsync(UserDTO userDTO)
    {
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            throw new CustomException(ErrorCode.Unauthorized, "User not authenticated.");
        }
        userDTO.UserId = int.Parse(userId);
        var result = await _userService.UpdateUserAsync(userDTO);
        ApiResponse<UserDTO> response = new ApiResponse<UserDTO>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }
}