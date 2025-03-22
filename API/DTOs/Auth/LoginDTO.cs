using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Auth;
public class LoginDTO
{
    [EmailAddress]
    public required string Email { get; set; }

    [DataType(DataType.Password)]
    public required string Password { get; set; }
}