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
        ApiResponse<PagedResponse<RoomDTO>> response = new ApiResponse<PagedResponse<RoomDTO>>(200, "Success", result);
        return Ok(response);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllRoomsNoPagingAsync()
    {
        var result = await _roomService.GetAllRoomsNoPagingAsync();
        ApiResponse<List<RoomDTO>> response = new ApiResponse<List<RoomDTO>>(200, "Success", result);
        return Ok(response);
    }

    [HttpGet("type/{roomType}")]
    public async Task<IActionResult> GetRoomsByType(string roomType)
    {
        var rooms = await _roomService.GetRoomsByTypeAsync(roomType);
        if (rooms == null || rooms.Count == 0)
        {
            return NotFound(new ApiResponse<string>(404, "No rooms found for the specified type", null).GetResponse());
        }
        return Ok(new ApiResponse<List<RoomDTO>>(200, "Success", rooms));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoomById(int id)
    {
        var result = await _roomService.GetRoomByIdAsync(id);
        if (result == null)
        {
            return NotFound(new ApiResponse<string>(404, "Room not found", null));
        }
        return Ok(new ApiResponse<RoomDTO>(200, "Success", result));
    }

    [HttpPost]
    public async Task<IActionResult> AddRoomAsync([FromForm] RoomDTO roomDTO, [FromForm] List<IFormFile>? imageFiles)
    {
        var result = await _roomService.AddRoomAsync(roomDTO, imageFiles);
        var response = new ApiResponse<RoomDTO>(201, "Room added successfully", result);
        return Ok(response);
    }



    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoomAsync(int id, [FromForm] RoomDTO roomDTO, [FromForm] List<IFormFile>? imageFiles)
    {
        var result = await _roomService.UpdateRoomAsync(id, roomDTO, imageFiles);
        return Ok(new ApiResponse<RoomDTO>(200, "Room updated successfully", result));
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoomAsync(int id)
    {
        await _roomService.DeleteRoomAsync(id);
        ApiResponse<string> response = new ApiResponse<string>(200, "Room deleted successfully", null);
        return Ok(response.GetResponse());
    }

    [HttpGet("isavailable/{roomId}")]
    public async Task<IActionResult> IsRoomAvailable(int roomId, [FromQuery] DateTime checkInDate, [FromQuery] DateTime checkOutDate)
    {
        try
        {
            bool isAvailable = await _roomService.IsRoomAvailableAsync(roomId, checkInDate, checkOutDate);
            return Ok(new ApiResponse<bool>(200, "Room is available", isAvailable));
        }
        catch (CustomException ex) when (ex.Code == ErrorCode.NotFound)
        {
            return NotFound(new ApiResponse<string>(404, ex.Message, null).GetResponse());
        }
    }


    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableRooms([FromQuery] DateTime checkInDate, [FromQuery] DateTime checkOutDate, [FromQuery] string roomType)
    {
        var result = await _roomService.GetAvailableRoomsAsync(checkInDate, checkOutDate, roomType);
        ApiResponse<List<RoomDTO>> response = new ApiResponse<List<RoomDTO>>(200, "Success", result);
        return Ok(response);
    }

    [HttpGet("available/all")]
    public async Task<IActionResult> GetAvailableRooms()
    {
        var result = await _roomService.GetAvailableRoomsAsync();
        ApiResponse<List<RoomDTO>> response = new ApiResponse<List<RoomDTO>>(200, "Success", result);
        return Ok(response);
    }
}
