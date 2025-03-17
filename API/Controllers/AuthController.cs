using API.DTOs;
using API.DTOs.Response;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(UserLoginDTO userLoginDTO)
    {
        Console.WriteLine("LoginAsync");
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _authService.LoginAsync(userLoginDTO);
        ApiResponse<string> response = new ApiResponse<string>(200, "Success", result);
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> AddUserAsync(UserRegisterDTO UserRegisterDTO)
    {
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _authService.AddUserAsync(UserRegisterDTO);
        ApiResponse<UserDTO> response = new ApiResponse<UserDTO>(200, "Success", result);
        return Ok(response);
    }
}