using API.DTOs;
using API.Models;
using API.Repositories;
using AutoMapper;

public class UserService {
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserDTO> AddUserAsync(UserRegisterDTO UserRegisterDTO)
    {
        var user = _mapper.Map<User>(UserRegisterDTO);
        var result = await _userRepository.AddUserAsync(user);
        return _mapper.Map<UserDTO>(result);
    }

    public async Task<IEnumerable<UserDTO>> GetAllUserAsync()
    {
        var result = await _userRepository.GetAllUserAsync();
        return _mapper.Map<IEnumerable<UserDTO>>(result);
    }
}