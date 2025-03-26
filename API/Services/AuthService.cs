using API.DTOs;
using API.Hashing;
using API.Models;
using API.Repositories;
using AutoMapper;
using API.DTOs.Response;
using StackExchange.Redis;
using IDatabase = StackExchange.Redis.IDatabase;
using API.DTOs.Auth;
using System.Security.Cryptography;

namespace API.Services;
public class AuthService
{
    private const string KeyAccessToken = "Access_Token_";
    private const string KeyRefreshToken = "Refresh_Token_";
    private const string KeyBlackListToken = "Black_List_";
    private const string KeyResetPassword = "Reset_Password_";
    private const string KeyOTP = "OTP_Email_";
    private const int AccessTokenExpiryMinutes = 15;
    private const int RefreshTokenExpiryDays = 10;
    private readonly IUserRepository _userRepository;
    private readonly BcryptService _bcryptService;
    private readonly IMapper _mapper;
    private readonly TokenService _jwtService;
    private readonly IDatabase _redis;
    private readonly EmailService _emailService;

    private readonly GoogleService _googleService;

    public AuthService(IUserRepository userRepository, TokenService jwtService, IMapper IMapper, IConnectionMultiplexer redis, EmailService emailService, GoogleService googleService)
    {
        _userRepository = userRepository;
        _bcryptService = new BcryptService();
        _jwtService = jwtService;
        _mapper = IMapper;
        _redis = redis.GetDatabase();
        _emailService = emailService;
        _googleService = googleService;
    }

    // Login user
    public async Task<AuthResponse> LoginAsync(LoginDTO loginDTO)
    {
        var user = await _userRepository.GetUserByEmailAsync(loginDTO.Email);
        if (user == null || user.PasswordHash == null || !_bcryptService.VerifyPassword(loginDTO.Password, user.PasswordHash))
        {
            throw new CustomException(ErrorCode.Unauthorized, "Invalid email or password.");
        }

        string accessToken = _jwtService.CreateToken(user);
        string refreshToken = _jwtService.CreateRefreshToken();

        string keyAccess = KeyAccessToken + user.UserId.ToString();
        string keyRefresh = KeyRefreshToken + user.UserId.ToString();

        _redis.StringSet(keyAccess, accessToken, TimeSpan.FromMinutes(AccessTokenExpiryMinutes));
        _redis.StringSet(keyRefresh, refreshToken, TimeSpan.FromDays(RefreshTokenExpiryDays));

        return new AuthResponse(accessToken, refreshToken);
    }

    // Generate new token via refresh
    public async Task<TokenResponse> RenewAccessToken(RefreshTokenRequest refreshTokenRequest)
    {
        string keyRefresh = KeyRefreshToken + refreshTokenRequest.UserId.ToString();
        var storedToken = _redis.StringGet(keyRefresh);

        if (string.IsNullOrEmpty(storedToken) || storedToken != refreshTokenRequest.RefreshToken)
        {
            throw new CustomException(ErrorCode.Unauthorized, "Invalid or expired refresh token.");
        }

        var user = await _userRepository.GetUserByIDAsync(refreshTokenRequest.UserId);
        if (user == null)
        {
            throw new CustomException(ErrorCode.NotFound, "User not found.");
        }

        string newAccessToken = _jwtService.CreateToken(user);
        string keyAccess = KeyAccessToken + user.UserId.ToString();
        _redis.StringSet(keyAccess, newAccessToken, TimeSpan.FromMinutes(AccessTokenExpiryMinutes));

        return new TokenResponse(newAccessToken);
    }

    // Add new user
    public async Task<UserDTO> AddUserAsync(UserRegisterDTO userRegisterDTO)
    {
        var userExist = await _userRepository.GetUserByEmailAsync(userRegisterDTO.Email);
        if (userExist != null)
        {
            throw new CustomException(ErrorCode.BadRequest, "Email already exists.");
        }

        var user = _mapper.Map<User>(userRegisterDTO);
        user.PasswordHash = _bcryptService.HashPassword(userRegisterDTO.PasswordHash);

        var result = await _userRepository.AddUserAsync(user);
        return _mapper.Map<UserDTO>(result);
    }

    // Forgot Password
    public async Task<string> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
    {
        var user = await _userRepository.GetUserByEmailAsync(forgotPasswordDTO.Email);
        if (user == null)
        {
            throw new CustomException(ErrorCode.NotFound, "Email does not exist.");
        }

        string OTP = GenerateSecureOTP();
        string key = KeyOTP + forgotPasswordDTO.Email;
        _redis.StringSet(key, OTP, TimeSpan.FromMinutes(5));

        try
        {
            await _emailService.SendOtpEmailAsync(forgotPasswordDTO.Email, OTP, user.FullName);
        }
        catch (Exception ex)
        {
            throw new CustomException(ErrorCode.InternalServerError, "Failed to send OTP email.", ex);
        }

        return "OTP has been sent. Please check your email.";
    }

    // Generate secure OTP
    private string GenerateSecureOTP()
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] bytes = new byte[6];
        rng.GetBytes(bytes);
        return string.Concat(bytes.Select(b => (b % 10).ToString()));
    }

    // Verify OTP 
    public async Task<TokenResponse?> VerifyOTP(VerifyOTP_DTO request)
    {
        string key = KeyOTP + request.Email;
        var storedOtp = await _redis.StringGetAsync(key);

        if (string.IsNullOrEmpty(storedOtp) || storedOtp != request.OTP)
        {
            throw new CustomException(ErrorCode.Unauthorized, "Invalid or expired OTP.");
        }

        await _redis.KeyDeleteAsync(key);

        var user = await _userRepository.GetUserByEmailAsync(request.Email);
        if (user == null)
        {
            throw new CustomException(ErrorCode.NotFound, "Email does not exist.");
        }

        string resetToken = _jwtService.CreateToken(user);
        string keyCreateResetPass = KeyResetPassword + request.Email;
        _redis.StringSet(keyCreateResetPass, resetToken, TimeSpan.FromMinutes(5));

        return new TokenResponse(resetToken);
    }

    public async Task<string> ResetPassword(ResetPasswordDTO request, string email)
    {
        string keyGetToken = KeyResetPassword + email;
        var token = _redis.StringGet(keyGetToken);

        if (string.IsNullOrEmpty(token))
        {
            throw new CustomException(ErrorCode.Unauthorized, "Invalid or expired reset password token.");
        }

        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null)
        {
            throw new CustomException(ErrorCode.NotFound, "Email does not exist.");
        }

        user.PasswordHash = _bcryptService.HashPassword(request.NewPassword);
        await _userRepository.UpdateUserAsync(user);
        await _redis.KeyDeleteAsync(keyGetToken);

        return "Password has been successfully reset.";
    }

    // Login with Google
    public async Task<AuthResponse> LoginWithGoogleAsync(GoogleLoginDTO googleLoginDTO)
    {
        var payload = await _googleService.VerifyGoogleTokenAsync(googleLoginDTO.IdToken);
        if (payload == null)
        {
            throw new CustomException(ErrorCode.Unauthorized, "Invalid Google ID token.");
        }

        var user = await _userRepository.GetUserByEmailAsync(payload.Email);
        if (user == null)
        {
            var newUser = new User
            {
                FullName = payload.Name,
                Email = payload.Email,
                CreatedDate = DateTime.Now
            };

            user = await _userRepository.AddUserAsync(newUser);
        }
        string accessToken = _jwtService.CreateToken(user);
        string refreshToken = _jwtService.CreateRefreshToken();

        string keyAccess = KeyAccessToken + user.UserId.ToString();
        string keyRefresh = KeyRefreshToken + user.UserId.ToString();

        _redis.StringSet(keyAccess, accessToken, TimeSpan.FromMinutes(AccessTokenExpiryMinutes));
        _redis.StringSet(keyRefresh, refreshToken, TimeSpan.FromDays(RefreshTokenExpiryDays));

        return new AuthResponse(accessToken, refreshToken);
    }
}
