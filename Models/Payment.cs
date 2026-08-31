using System;
using System.Collections.Generic;

namespace HotelReservation.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int ReservationId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public string? TransactionReference { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual Reservation Reservation { get; set; } = null!;
}
