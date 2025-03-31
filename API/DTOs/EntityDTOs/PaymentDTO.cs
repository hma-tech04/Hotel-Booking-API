namespace API.DTOs.EntityDTOs;

public class PaymentDTO
{
    public int PaymentId { get; set; }

    public int BookingId { get; set; }

    public DateTime? PaymentDate { get; set; }

    public decimal PaymentAmount { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;
}
