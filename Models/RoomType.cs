using System;
using System.Collections.Generic;

namespace HotelReservation.Models;

public partial class RoomType
{
    public int RoomTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int MaxGuests { get; set; }

    public decimal BasePrice { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
