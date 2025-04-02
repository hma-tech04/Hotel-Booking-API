using API.Enum;

namespace API.DTOs.EntityDTOs;

public class BookingDTO
{
    public int BookingId { get; set; }

    public int UserId { get; set; }

    public int RoomId { get; set; }

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }
    
    public DateTime? ActualCheckInTime { get; set; }  

    public DateTime? ActualCheckOutTime { get; set; } 

    public decimal? TotalPrice { get; set; }

    public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;

    public int? PaymentId { get; set; }
}
