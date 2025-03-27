using API.DTOs;
using API.Models;
using API.Repositories;
using AutoMapper;
using API.Hashing;
using API.DTOs.EntityDTOs;
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

    // Get user by ID
    public async Task<UserDTO> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetUserByIDAsync(id);
        if (user == null)
        {
            throw new CustomException(ErrorCode.NotFound, $"No user found with ID: {id}");
        }
        return _mapper.Map<UserDTO>(user);
    }

    // Update user
    public async Task<UserDTO> UpdateUserAsync(UserDTO userDTO)
    {
        var existingUser = await _userRepository.GetUserByIDAsync(userDTO.UserId);
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
