namespace API.DTOs.EntityDTOs;

public class RoomRequestDTO
{
    public int RoomId { get; set; }

    public string RoomType { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public required IFormFile ThumbnailUrl { get; set; }

    public bool? IsAvailable { get; set; }

    public List<string> RoomImages { get; set; } = new List<string>();
}
