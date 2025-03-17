using System;
using System.Collections.Generic;
using API.Enum;

namespace API.Models;

public partial class User
{
    public int UserId { get; set; }

    public required string FullName { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public UserRole Role { get; set; } = UserRole.User;

    public DateTime? CreatedDate { get; set; }

    public required string PhoneNumber { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
