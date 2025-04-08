using System.Security.Claims;
using API.DTOs.Auth;
using API.DTOs.EntityDTOs;
using API.DTOs.Response;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }


    // Login account 
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginDTO loginDTO)
    {
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _authService.LoginAsync(loginDTO);
        ApiResponse<AuthResponse> response = new ApiResponse<AuthResponse>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }

    // Register a new account
    [HttpPost("register")]
    public async Task<IActionResult> AddUserAsync(UserRegisterDTO UserRegisterDTO)
    {
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _authService.AddUserAsync(UserRegisterDTO);
        ApiResponse<UserDTO> response = new ApiResponse<UserDTO>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }

    // Renew the access token when it has expired using the refresh token.
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RenewToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        Console.WriteLine($"RefreshToken: {refreshToken}");
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new CustomException(ErrorCode.Unauthorized, "Token is invalid or has expired.");
        }

        var userId = _authService.GetUserIdFromRefreshToken(refreshToken);
        Console.WriteLine($"UserId: {userId}");
        
        if (string.IsNullOrEmpty(userId.ToString()))
        {
            throw new CustomException(ErrorCode.Unauthorized, "Token is invalid or has expired.");
        }

        var refreshTokenRequest = new RefreshTokenRequest(userId, refreshToken);
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        
        var result = await _authService.RenewAccessToken(refreshTokenRequest);
        return Ok(new ApiResponse<TokenResponse>(ErrorCode.OK, "Success", result));
    }

    // Handle forgot password request
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
    {
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _authService.ForgotPassword(forgotPasswordDTO);
        ApiResponse<string> response = new ApiResponse<string>(ErrorCode.OK, result);
        return Ok(response.GetResponse());
    }

    // Verify otp from client
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOTP(VerifyOTP_DTO request)
    {
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _authService.VerifyOTP(request);
        ApiResponse<TokenResponse> response;
        if (result == null)
        {
            response = new ApiResponse<TokenResponse>(ErrorCode.Unauthorized, "OTP is invalid or has expired.", null);
        }
        else
        {
            response = new ApiResponse<TokenResponse>(ErrorCode.OK, "Success", result);
        }
        return Ok(response.GetResponse());
    }

    // Reset the user's password after verifying OTP or other authentication methods
    [Authorize]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDTO request)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (email == null)
        {
            throw new CustomException(ErrorCode.BadRequest, "Token is invalid or has expired.");
        }
        var result = await _authService.ResetPassword(request, email);
        ApiResponse<string> response = new ApiResponse<string>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }

    // Login account with google
    [HttpPost("login-google")]
    public async Task<IActionResult> LoginWithGoogle(GoogleLoginDTO googleLoginDTO)
    {
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _authService.LoginWithGoogleAsync(googleLoginDTO);
        ApiResponse<AuthResponse> response = new ApiResponse<AuthResponse>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }

    // Logout account
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAccount()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new CustomException(ErrorCode.Unauthorized, "Token is invalid or has expired.");
        }

        var userId = _authService.GetUserIdFromRefreshToken(refreshToken);
        if (string.IsNullOrEmpty(userId.ToString()))
        {
            throw new CustomException(ErrorCode.Unauthorized, "Token is invalid or has expired.");
        }

        var refreshTokenRequest = new RefreshTokenRequest(userId, refreshToken);
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }

        var result = await _authService.Logout(refreshTokenRequest);
        return Ok(new ApiResponse<bool>(ErrorCode.OK, "Success", result));
    }
}