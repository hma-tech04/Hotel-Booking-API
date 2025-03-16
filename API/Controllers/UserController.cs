using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using API.DTOs.Response;

namespace API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUserAsync()
    {
        var result = await _userService.GetAllUserAsync();
        ApiResponse<IEnumerable<UserDTO>> response = new ApiResponse<IEnumerable<UserDTO>>(200, "Success", result);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> AddUserAsync(UserRegisterDTO UserRegisterDTO)
    {
        var result = await _userService.AddUserAsync(UserRegisterDTO);
        ApiResponse<UserDTO> response = new ApiResponse<UserDTO>(200, "Success", result);
        return Ok(response);
    }
}