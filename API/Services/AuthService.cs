using API.DTOs;
using API.Hashing;
using API.Models;
using API.Repositories;
using AutoMapper;

namespace API.Services;
public class AuthService{

    private readonly IUserRepository _userRepository;
    private readonly BcryptService _bcryptService;
    private readonly IMapper _mapper;
    private readonly TokenService _jwtService;
    public AuthService(IUserRepository userRepository, TokenService jwtService, IMapper IMapper)
    {
        _userRepository = userRepository;
        _bcryptService = new BcryptService();
        _jwtService = jwtService;
        _mapper = IMapper;
    }

    // Login user
    public async Task<string> LoginAsync(UserLoginDTO userLoginDTO)
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

        return _jwtService.CreateToken(user);
    }

    // Add new user
    public async Task<UserDTO> AddUserAsync(UserRegisterDTO userRegisterDTO)
    {
        Console.WriteLine("AddUserAsync" + userRegisterDTO.Email);
        var userExist = await _userRepository.GetUserByEmailAsync(userRegisterDTO.Email);
        if (userExist != null)
        {
            throw new CustomException(ErrorCode.BadRequest, "Email already exists.");
        }

        var user = _mapper.Map<User>(userRegisterDTO);
        string PasswordHash = _bcryptService.HashPassword(user.PasswordHash);
        user.PasswordHash = PasswordHash;
        var result = await _userRepository.AddUserAsync(user);

        return _mapper.Map<UserDTO>(result);
    }
}