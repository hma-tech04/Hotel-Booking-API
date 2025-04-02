using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Statistics;

public class PhoneNumberRequestDto
{
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits.")]
    public required string PhoneNumber { get; set; }
}