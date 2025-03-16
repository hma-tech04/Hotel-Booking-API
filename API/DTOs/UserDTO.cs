using API.Enum;

namespace API.DTOs;

public class UserDTO
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public DateTime? CreatedDate { get; set; }
}
