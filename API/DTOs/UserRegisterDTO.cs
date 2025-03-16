using API.Enum;

namespace API.DTOs;

public class UserRegisterDTO
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string PhoneNumber { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime? CreatedDate { get; set; } = System.DateTime.Now;
}
