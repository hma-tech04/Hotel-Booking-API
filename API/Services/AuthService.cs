using API.DTOs;
using API.Hashing;
using API.Models;
using API.Repositories;
using AutoMapper;
using API.DTOs.Response;
using StackExchange.Redis;
using IDatabase = StackExchange.Redis.IDatabase;
using API.DTOs.Auth;

namespace API.Services;
public class AuthService
{
    private const string KeyAccessToken = "Access_Token_";
    private const string KeyRefreshToken = "Refresh_Token_";
    private const string KeyBlackListToken = "Black_List_";
    private const string KeyOTP = "OTP_Email_";
    private const int AccessTokenExpiryMinutes = 15;
    private const int RefreshTokenExpiryDays = 10;
    private readonly IUserRepository _userRepository;
    private readonly BcryptService _bcryptService;
    private readonly IMapper _mapper;
    private readonly TokenService _jwtService;
    private readonly IDatabase _redis;
    private readonly EmailService _emailService;

    public AuthService(IUserRepository userRepository, TokenService jwtService, IMapper IMapper, IConnectionMultiplexer redis, EmailService emailService)
    {
        _userRepository = userRepository;
        _bcryptService = new BcryptService();
        _jwtService = jwtService;
        _mapper = IMapper;
        _redis = redis.GetDatabase();
        _emailService = emailService;
    }

    // Login user
    public async Task<AuthResponse> LoginAsync(UserLoginDTO userLoginDTO)
    {
        var user = await _userRepository.GetUserByEmailAsync(userLoginDTO.Email);
        if (user == null)
        {
            throw new CustomException(ErrorCode.NotFound, "Email not found.");
        }

        if (!_bcryptService.VerifyPassword(userLoginDTO.Password, user.PasswordHash))
        {
            throw new CustomException(ErrorCode.BadRequest, "Password is incorrect.");
        }

        string accessToken = _jwtService.CreateToken(user);
        string refreshToken = _jwtService.CreateRefreshToken();

        string keyAccess = KeyAccessToken + user.UserId.ToString();
        string keyRefresh = KeyRefreshToken + user.UserId.ToString();

        try
        {
            _redis.StringSet(keyAccess, accessToken, TimeSpan.FromMinutes(AccessTokenExpiryMinutes));
            _redis.StringSet(keyRefresh, refreshToken, TimeSpan.FromDays(RefreshTokenExpiryDays));
        }
        catch (Exception ex)
        {
            throw new CustomException(ErrorCode.InternalServerError, "Unable to store token in Redis.", ex);
        }

        return new AuthResponse(accessToken, refreshToken);
    }

    // Generate new token via refresh
    public async Task<TokenResponse> RenewAccessToken(RefreshTokenRequest refreshTokenRequest)
    {
        try
        {

            string keyRefresh = KeyRefreshToken + refreshTokenRequest.UserId.ToString();
            var storedToken = _redis.StringGet(keyRefresh);

            if (string.IsNullOrEmpty(storedToken) || storedToken != refreshTokenRequest.RefreshToken)
            {
                throw new CustomException(ErrorCode.Unauthorized, "Invalid or expired refresh token.");
            }

            var user = await _userRepository.GetUserByIDAsync(int.Parse(refreshTokenRequest.UserId.ToString()));
            if (user == null)
            {
                throw new CustomException(ErrorCode.NotFound, "User not found.");
            }

            string newAccessToken = _jwtService.CreateToken(user);
            string keyAccess = KeyAccessToken + user.UserId.ToString();
            _redis.StringSet(keyAccess, newAccessToken, TimeSpan.FromMinutes(AccessTokenExpiryMinutes));

            return new TokenResponse(newAccessToken);
        }
        catch (Exception ex)
        {
            throw new CustomException(ErrorCode.InternalServerError, "Error while renewing access token.", ex);
        }
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
        string passwordHash = _bcryptService.HashPassword(userRegisterDTO.PasswordHash);
        user.PasswordHash = passwordHash;
        var result = await _userRepository.AddUserAsync(user);

        return _mapper.Map<UserDTO>(result);
    }

    // Forgot Password
    public async Task<string> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
    {
        var user = await _userRepository.GetUserByEmailAsync(forgotPasswordDTO.Email);

        if (user == null)
        {
            throw new CustomException(ErrorCode.NotFound, "Email does not exist in the system.");
        }

        string OTP = GenerateOTP();

        string email = forgotPasswordDTO.Email;

        // Khởi tạo Task để gửi email bất đồng bộ
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendOtpEmailAsync(email, OTP, user.FullName);
            }
            catch (Exception ex)
            {
                throw new CustomException(ErrorCode.BadRequest, "An error occurred during the email sending process.", ex);
            }
        });

        string key = KeyOTP + email;
        _redis.StringSet(key, OTP, TimeSpan.FromMinutes(5));


        return "OTP has been sent, please check your email.";
    }

    // Generate OTP
    private string GenerateOTP()
    {
        Random random = new Random();
        string otp = "";

        for (int i = 0; i < 6; i++)
        {
            otp += random.Next(0, 10);
        }

        return otp;
    }

    // Verify OTP 
    public async Task<ApiResponse<string>> VerifyOTP(VerifyOTP_DTO request)
    {
        string key = KeyOTP + request.Email;

        var OTP = await _redis.StringGetAsync(key);

        if (string.IsNullOrEmpty(OTP) || OTP != request.OTP)
        {
            return new ApiResponse<string>(400, "OTP is invalid or has expired.");
        }

        return new ApiResponse<string>(200, "OTP verification successful.", "Success");
    }

}
