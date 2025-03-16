using System;
using System.Collections.Generic;
using API.Enum;

namespace API.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public UserRole Role { get; set; } = UserRole.User;

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
