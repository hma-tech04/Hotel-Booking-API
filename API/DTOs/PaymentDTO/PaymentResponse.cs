namespace API.DTOs.PaymentDTO;
public class PaymentResponse
{
    public bool IsSuccess { get; set; }
    public required string Message { get; set; }
}