namespace API.DTOs;

public class RoomImageDTO
{
    public int ImageId { get; set; }

    public int RoomId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
}
