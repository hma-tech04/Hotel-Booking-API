using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using API.DTOs.Response;
using API.Services;
using Microsoft.AspNetCore.Http;

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

    [HttpGet]
    public async Task<IActionResult> GetAllRoomsAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _roomService.GetAllRoomsAsync(pageNumber, pageSize);
        ApiResponse<PagedResponse<RoomDTO>> response = new ApiResponse<PagedResponse<RoomDTO>>(200, "Success", result);
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
    
    [HttpGet("type/{roomType}")]
    public async Task<IActionResult> GetRoomsByType(string roomType)
    {
        var rooms = await _roomService.GetRoomsByTypeAsync(roomType);
        if (rooms == null || rooms.Count == 0)
        {
            return NotFound("No rooms found with the specified type.");
        }
        return Ok(rooms);
    }

    // Thêm phòng (hỗ trợ upload ảnh)
    [HttpPost]
    public async Task<IActionResult> AddRoomAsync([FromForm] RoomDTO roomDTO, [FromForm] IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }

        var result = await _roomService.AddRoomAsync(roomDTO, imageFile);
        ApiResponse<RoomDTO> response = new ApiResponse<RoomDTO>(201, "Room created successfully", result);
        return CreatedAtAction(nameof(GetRoomByIdAsync), new { id = result.RoomId }, response);
    }

    // Cập nhật phòng (hỗ trợ upload ảnh)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoomAsync(int id, [FromForm] RoomDTO roomDTO, [FromForm] IFormFile? imageFile)
    {
        if (id != roomDTO.RoomId)
            return BadRequest(new ApiResponse<string>(400, "Room ID mismatch", null));

        if (!ModelState.IsValid)
        {
            throw new CustomException(ErrorCode.BadRequest, "Invalid input");
        }

        var result = await _roomService.UpdateRoomAsync(id, roomDTO, imageFile);
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
