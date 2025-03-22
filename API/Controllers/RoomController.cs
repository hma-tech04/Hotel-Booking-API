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
            return NotFound("No rooms found with the specified type.");
        }
        return Ok(rooms);
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
    public async Task<IActionResult> AddRoomAsync([FromBody] RoomDTO roomDTO)
    {
        Console.WriteLine($"[DEBUG] Received RoomType = {roomDTO.RoomType}");
        Console.WriteLine($"[DEBUG] Received Price = {roomDTO.Price}");
        Console.WriteLine($"[DEBUG] Received Description = {roomDTO.Description}");
        Console.WriteLine($"[DEBUG] Received IsAvailable = {roomDTO.IsAvailable}");

        if (string.IsNullOrWhiteSpace(roomDTO.RoomType))
        {
            return BadRequest(new ApiResponse<string>(400, "RoomType is required.", null));
        }
        if (roomDTO.Price <= 0)
        {
            return BadRequest(new ApiResponse<string>(400, "Price must be greater than 0.", null));
        }

        var result = await _roomService.AddRoomAsync(roomDTO);
        return CreatedAtAction(nameof(GetRoomById), new { id = result.RoomId }, new ApiResponse<RoomDTO>(201, "Room created successfully", result));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoomAsync(int id, [FromBody] RoomDTO roomDTO)
    {
        if (id != roomDTO.RoomId)
            return BadRequest(new ApiResponse<string>(400, "Room ID mismatch", null));

        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<string>(400, "Invalid input", null));
        }

        var result = await _roomService.UpdateRoomAsync(id, roomDTO);
        return Ok(new ApiResponse<RoomDTO>(200, "Room updated successfully", result));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoomAsync(int id)
    {
        await _roomService.DeleteRoomAsync(id);
        ApiResponse<string> response = new ApiResponse<string>(200, "Room deleted successfully", null);
        return Ok(response);
    }
}

/*
 [HttpPost]
    public async Task<IActionResult> AddRoomAsync(
        [FromForm] string? roomType,
        [FromForm] string? price,
        [FromForm] string? description,
        [FromForm] bool isAvailable,
        [FromForm] IFormFile? imageFile)
    {
        Console.WriteLine($"[DEBUG] Received roomType = '{roomType}'");
        Console.WriteLine($"[DEBUG] Received price = '{price}'");
        Console.WriteLine($"[DEBUG] Received description = '{description}'");
        Console.WriteLine($"[DEBUG] Received isAvailable = '{isAvailable}'");
        Console.WriteLine($"[DEBUG] Received imageFile = '{imageFile?.FileName}'");

        if (string.IsNullOrWhiteSpace(roomType))
        {
            return BadRequest(new ApiResponse<string>(400, "RoomType is required.", null));
        }
        if (string.IsNullOrWhiteSpace(price) 
            || !decimal.TryParse(price, out decimal parsedPrice) 
            || parsedPrice <= 0)
        {
            return BadRequest(new ApiResponse<string>(400, "Price must be greater than 0.", null));
        }

        var roomDTO = new RoomDTO
        {
            RoomType = roomType,
            Price = parsedPrice,
            Description = description,
            IsAvailable = isAvailable
        };

        var result = await _roomService.AddRoomAsync(roomDTO, imageFile);
        Console.WriteLine($"[DEBUG] Created new room with ID = {result.RoomId}");
        return CreatedAtAction(nameof(GetRoomById), new { id = result.RoomId }, result);
    }

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
    */