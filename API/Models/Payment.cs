using System;
using System.Collections.Generic;
using API.Enum;

namespace API.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int BookingId { get; set; }

    public DateTime? PaymentDate { get; set; }

    public decimal PaymentAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Momo;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public virtual Booking Booking { get; set; } = null!;
}
