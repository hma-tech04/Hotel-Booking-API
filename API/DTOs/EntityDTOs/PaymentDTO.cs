using API.Enum;

namespace API.DTOs.EntityDTOs;

public class PaymentDTO
{
    public int PaymentId { get; set; }

    public int BookingId { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.Now;

    public decimal PaymentAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.VNPAY;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
}
