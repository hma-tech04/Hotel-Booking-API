using Microsoft.AspNetCore.Mvc;
using API.DTOs.Response;
using API.DTOs.EntityDTOs;

namespace API.Controllers;
[ApiController]
[Route("api/rooms")]
public class RoomController : ControllerBase
{
    private readonly RoomService _roomService;

    public RoomController(RoomService roomService)
    {
        _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoomsAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _roomService.GetAllRoomsAsync(pageNumber, pageSize);
        ApiResponse<PagedResponse<RoomDTO>> response = new ApiResponse<PagedResponse<RoomDTO>>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllRoomsNoPagingAsync()
    {
        var result = await _roomService.GetAllRoomsNoPagingAsync();
        ApiResponse<List<RoomDTO>> response = new ApiResponse<List<RoomDTO>>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }

    [HttpGet("type/{roomType}")]
    public async Task<IActionResult> GetRoomsByType(string roomType)
    {
        var rooms = await _roomService.GetRoomsByTypeAsync(roomType);
        if (rooms == null || rooms.Count == 0)
        {
            return NotFound(new ApiResponse<string>(ErrorCode.NotFound, "No rooms found for the specified type", null).GetResponse());
        }
        return Ok(new ApiResponse<List<RoomDTO>>(ErrorCode.OK, "Success", rooms));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoomById(int id)
    {
        var result = await _roomService.GetRoomByIdAsync(id);
        if (result == null)
        {
            return NotFound(new ApiResponse<string>(ErrorCode.NotFound, "Room not found", null));
        }
        return Ok(new ApiResponse<RoomDTO>(ErrorCode.OK, "Success", result));
    }

    [HttpPost]
    public async Task<IActionResult> AddRoomAsync([FromForm] RoomRequestDTO roomDTO, [FromForm] List<IFormFile>? imageFiles)
    {
        var result = await _roomService.AddRoomAsync(roomDTO, imageFiles);
        var response = new ApiResponse<RoomDTO>(ErrorCode.OK, "Room added successfully", result);
        return Ok(response);
    }



    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoomAsync(int id, [FromForm] RoomRequestDTO roomRequestDTO, [FromForm] List<IFormFile>? imageFiles)
    {
        var result = await _roomService.UpdateRoomAsync(id, roomRequestDTO, imageFiles);
        return Ok(new ApiResponse<RoomDTO>(ErrorCode.OK, "Room updated successfully", result));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoomAsync(int id)
    {
        await _roomService.DeleteRoomAsync(id);
        ApiResponse<string> response = new ApiResponse<string>(ErrorCode.OK, "Room deleted successfully", null);
        return Ok(response.GetResponse());
    }

    [HttpGet("isavailable/{roomId}")]
    public async Task<IActionResult> IsRoomAvailable(int roomId, [FromQuery] DateTime checkInDate, [FromQuery] DateTime checkOutDate)
    {
        try
        {
            bool isAvailable = await _roomService.IsRoomAvailableAsync(roomId, checkInDate, checkOutDate);
            return Ok(new ApiResponse<bool>(ErrorCode.OK, "Room is available", isAvailable));
        }
        catch (CustomException ex) when (ex.Code == ErrorCode.NotFound)
        {
            return NotFound(new ApiResponse<string>(ErrorCode.BadRequest, ex.Message, null).GetResponse());
        }
    }


    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableRooms([FromQuery] DateTime checkInDate, [FromQuery] DateTime checkOutDate, [FromQuery] string roomType)
    {
        var result = await _roomService.GetAvailableRoomsAsync(checkInDate, checkOutDate, roomType);
        ApiResponse<List<RoomDTO>> response = new ApiResponse<List<RoomDTO>>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }

    [HttpGet("available/all")]
    public async Task<IActionResult> GetAvailableRooms()
    {
        var result = await _roomService.GetAvailableRoomsAsync();
        ApiResponse<List<RoomDTO>> response = new ApiResponse<List<RoomDTO>>(ErrorCode.OK, "Success", result);
        return Ok(response);
    }
}
