using API.DTOs;
using API.Models;
using API.Repositories;
using AutoMapper;
using API.Hashing;
namespace API.Services;
public class UserService 
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly BcryptService _bcryptService;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _bcryptService = new BcryptService();
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

    // Get all users
    public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUserAsync();
        return _mapper.Map<IEnumerable<UserDTO>>(users);
    }

    // Get user by ID
    public async Task<UserDTO> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetUseByIDAsync(id);
        if (user == null)
        {
            throw new CustomException(ErrorCode.NotFound, $"No user found with ID: {id}");
        }

        return _mapper.Map<UserDTO>(user);
    }

    // Update user
    public async Task<UserDTO> UpdateUserAsync(UserDTO userDTO)
{
    var existingUser = await _userRepository.GetUseByIDAsync(userDTO.UserId);
    if (existingUser == null)
    {
        throw new CustomException(ErrorCode.NotFound, $"No user found with ID: {userDTO.UserId}");
    }

    if (existingUser.Email != userDTO.Email)
    {
        var userWithEmailExist = await _userRepository.GetUserByEmailAsync(userDTO.Email);
        if (userWithEmailExist != null)
        {
            throw new CustomException(ErrorCode.BadRequest, "Email already exists.");
        }
    }

    var userToUpdate = _mapper.Map<User>(userDTO);
    userToUpdate.CreatedDate = existingUser.CreatedDate;
    userToUpdate.PasswordHash = existingUser.PasswordHash;
    var updatedUser = await _userRepository.UpdateUserAsync(userToUpdate);

    return _mapper.Map<UserDTO>(updatedUser);
}


    // Delete user
    public async Task<UserDTO> DeleteUserAsync(int id)
    {
        var user = await _userRepository.DeleteUserAsync(id);
        if (user == null)
        {
            throw new CustomException(ErrorCode.NotFound, $"No user found with ID: {id}");
        }

        return _mapper.Map<UserDTO>(user);
    }

    // Get user by email
    public async Task<UserDTO> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null)
        {
            throw new CustomException(ErrorCode.NotFound, $"No user found with email: {email}");
        }

        return _mapper.Map<UserDTO>(user);
    }
}
