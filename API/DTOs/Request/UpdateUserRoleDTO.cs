using API.Enum;

namespace API.DTOs.Request;
public class UpdateUserRoleDTO
{
    public required UserRole Role { get; set; }
}
