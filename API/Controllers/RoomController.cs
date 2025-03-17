using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using API.DTOs.Response;
using API.Services;

namespace API.Controllers;
[ApiController]
[Route("api/rooms")]
public class RoomController : ControllerBase
{
    private readonly RoomService _roomService;

    public RoomController(RoomService roomService)
    {
        _roomService = roomService;
    }

    // Lấy danh sách phòng
    [HttpGet]
    public async Task<IActionResult> GetAllRoomsAsync()
    {
        var result = await _roomService.GetAllRoomsAsync();
        ApiResponse<IEnumerable<RoomDTO>> response = new ApiResponse<IEnumerable<RoomDTO>>(200, "Success", result);
        return Ok(response);
    }

    // Lấy chi tiết phòng theo ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoomByIdAsync(int id)
    {
        var result = await _roomService.GetRoomByIdAsync(id);
        ApiResponse<RoomDTO> response = new ApiResponse<RoomDTO>(200, "Success", result);
        return Ok(response);
    }

    // Thêm phòng
    [HttpPost]
    public async Task<IActionResult> AddRoomAsync(RoomDTO roomDTO)
    {
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _roomService.AddRoomAsync(roomDTO);
        ApiResponse<RoomDTO> response = new ApiResponse<RoomDTO>(201, "Room created successfully", result);
        return CreatedAtAction(nameof(GetRoomByIdAsync), new { id = result.RoomId }, response);
    }

    // Cập nhật phòng
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoomAsync(int id, RoomDTO roomDTO)
    {
        if (id != roomDTO.RoomId)
            return BadRequest(new ApiResponse<string>(400, "Room ID mismatch", null));

        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }
        var result = await _roomService.UpdateRoomAsync(roomDTO);
        ApiResponse<RoomDTO> response = new ApiResponse<RoomDTO>(200, "Room updated successfully", result);
        return Ok(response);
    }

    // Xóa phòng
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoomAsync(int id)
    {
        await _roomService.DeleteRoomAsync(id);
        ApiResponse<string> response = new ApiResponse<string>(200, "Room deleted successfully", null);
        return Ok(response);
    }
}
