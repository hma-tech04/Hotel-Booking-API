namespace API.DTOs.PaymentDTO;
public class PaymentRequest
{
    public required int Amount { get; set; }
    public  required string OrderId { get; set; }
}    