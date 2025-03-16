using API.Enum;

namespace API.DTOs;

public class UserDTO
{
    public int UserId { get; set; }

    public required string FullName { get; set; }

    public required string PhoneNumber { get; set; }

    public required string Email { get; set; }

    public UserRole Role { get; set; }

    public DateTime? CreatedDate { get; set; }
}
