using System.ComponentModel.DataAnnotations;
namespace API.DTOs.Auth;
public class ResetPasswordDTO
{
    [DataType(DataType.Password)]
    public required string NewPassword { get; set; }
}