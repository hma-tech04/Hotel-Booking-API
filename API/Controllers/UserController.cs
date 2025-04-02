using Microsoft.AspNetCore.Mvc;
using API.DTOs.Response;
using API.Services;
using API.DTOs.EntityDTOs;

namespace API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("id/{id}")]
    public async Task<IActionResult> GetUserByIDAsync(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        ApiResponse<UserDTO> response = new ApiResponse<UserDTO>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetUserByEmailAsync(string email)
    {
        var result = await _userService.GetUserByEmailAsync(email);
        ApiResponse<UserDTO> response = new ApiResponse<UserDTO>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUserAsync(UserDTO userDTO)
    {
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _userService.UpdateUserAsync(userDTO);
        ApiResponse<UserDTO> response = new ApiResponse<UserDTO>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }
}