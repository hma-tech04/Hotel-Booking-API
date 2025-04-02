
using API.Enum;

namespace API.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int UserId { get; set; }

    public int RoomId { get; set; }

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime? ActualCheckInTime { get; set; }  
    public DateTime? ActualCheckOutTime { get; set; } 

    public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;

    public int? PaymentId { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Room Room { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
