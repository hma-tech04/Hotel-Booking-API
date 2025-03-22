using API.DTOs;
using API.DTOs.Auth;
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
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _authService.LoginAsync(userLoginDTO);
        ApiResponse<AuthResponse> response = new ApiResponse<AuthResponse>(200, "Success", result);
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

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RenewToken(RefreshTokenRequest refreshTokenRequest){
        if(!ModelState.IsValid){
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _authService.RenewAccessToken(refreshTokenRequest);
        ApiResponse<TokenResponse> response = new ApiResponse<TokenResponse>(200, "Success", result);
        return Ok(response);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO){
        if(!ModelState.IsValid){
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _authService.ForgotPassword(forgotPasswordDTO);
        ApiResponse<string> response = new ApiResponse<string> (200, result);
        return Ok(response.GetResponse());
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOTP(VerifyOTP_DTO request)
    {
        if(!ModelState.IsValid){
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var response = await _authService.VerifyOTP(request);
        return Ok(response.GetResponse());
    }
}