using API.Enum;

namespace API.DTOs;

public class UserRegisterDTO
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime? CreatedDate { get; set; } = System.DateTime.Now;
}
