namespace API.DTOs;

public class BookingDTO
{
    public int BookingId { get; set; }

    public int UserId { get; set; }

    public int RoomId { get; set; }

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public decimal TotalPrice { get; set; }

    public string BookingStatus { get; set; } = string.Empty;

    public int? PaymentId { get; set; }
}
