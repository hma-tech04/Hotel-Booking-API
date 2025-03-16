using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string RoomType { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string? ThumbnailUrl { get; set; }

    public bool? IsAvailable { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<RoomImage> RoomImages { get; set; } = new List<RoomImage>();
}
