
using API.DTOs;
using API.Repositories;
using AutoMapper;

public class UserAdminService{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserAdminService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    // Get all users
    public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUserAsync();
        return _mapper.Map<IEnumerable<UserDTO>>(users);
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

    // Update role user
    public async Task<UserDTO> UpdateRoleUserAsync(int id, UpdateUserRoleDTO updateUserRoleDTO)
    {
        var existingUser = await _userRepository.GetUseByIDAsync(id);
        if (existingUser == null)
        {
            throw new CustomException(ErrorCode.NotFound, $"No user found with ID: {id}");
        }

        existingUser.Role = updateUserRoleDTO.Role;
        var result = await _userRepository.UpdateUserAsync(existingUser);
        return _mapper.Map<UserDTO>(result);
    }
}