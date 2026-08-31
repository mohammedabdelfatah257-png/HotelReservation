using System;
using System.Collections.Generic;

namespace HotelReservation.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public int HotelId { get; set; }

    public int RoomTypeId { get; set; }

    public string RoomNumber { get; set; } = null!;

    public int Floor { get; set; }

    public string Status { get; set; } = null!;

    public decimal PricePerNight { get; set; }

    public bool IsActive { get; set; }

    public virtual Hotel Hotel { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual RoomType RoomType { get; set; } = null!;
}
