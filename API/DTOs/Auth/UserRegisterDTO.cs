using API.Enum;

namespace API.DTOs.Auth;

public class UserRegisterDTO
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime? CreatedDate { get; set; } = System.DateTime.Now;
}
