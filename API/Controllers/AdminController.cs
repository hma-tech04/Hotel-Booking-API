using API.DTOs.EntityDTOs;
using API.DTOs.Request;
using API.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[ApiController]
[Route("api/[controller]")] 
public class AdminController : ControllerBase{
    private readonly UserAdminService _userAdminService;

    public AdminController(UserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    // Get all users
    [HttpGet("users")]
    public async Task<IActionResult> GetAllAdminAsync()
    {
        var result = await _userAdminService.GetAllUsersAsync();
        ApiResponse<IEnumerable<UserDTO>> response = new ApiResponse<IEnumerable<UserDTO>>(200, "Success", result);
        return Ok(response);
    }


    // Delete user by ID
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUserByIDAsync(int id){
        var result = await _userAdminService.DeleteUserAsync(id);
        ApiResponse<UserDTO> response = new ApiResponse<UserDTO>(200, "Success", result);
        return Ok(response);
    }

    // Update role user
    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> UpdateRoleUserAsync(int id, UpdateUserRoleDTO updateUserRoleDTO)
    {
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _userAdminService.UpdateRoleUserAsync(id, updateUserRoleDTO);
        ApiResponse<UserDTO> response = new ApiResponse<UserDTO>(200, "Success", result);
        return Ok(response);
    }
}