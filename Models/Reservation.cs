using System;
using System.Collections.Generic;

namespace HotelReservation.Models;

public partial class Reservation
{
    public int ReservationId { get; set; }

    public int UserId { get; set; }



    public int RoomId { get; set; }

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public int GuestsCount { get; set; }

    public string Status { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Room Room { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
