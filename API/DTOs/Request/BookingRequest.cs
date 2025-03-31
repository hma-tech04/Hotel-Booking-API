using System.ComponentModel.DataAnnotations;
using API.Enum;

namespace API.DTOs.Request;

public class BookingRequest
{
    public int BookingId { get; set; }

    public int UserId { get; set; }

    public int RoomId { get; set; }

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public decimal TotalPrice { get; set; }
    
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be 10 digits.")]
    public required string PhoneNumber { get; set; }

    public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;

    public int? PaymentId { get; set; }
}
