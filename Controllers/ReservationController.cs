using HotelReservation.DTOs;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly HotelReservationDbContext _context;

        public ReservationController(HotelReservationDbContext context)
        {

            _context = context;
        }

        [HttpGet("AllReservations")]
        [Authorize(Policy = "ViewReservations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllReservations()
        {
            var reservations = await _context.Reservations
                .ToListAsync();

            if (reservations.Count == 0)
            {
                return NotFound("No reservations found.");
            }

            return Ok(reservations);
        }

        [HttpGet("ReservationById/{id}")]
        [Authorize(Policy = "ViewReservations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReservationById(int id)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return Ok(reservation);
        }

        [HttpPost("CreateReservation")]
        [Authorize(Policy = "CreateReservation")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddReservation(AddReservationDto dto)
        {
            // 1. Get current UserId from JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);


            // 2. Check User exists and is active
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.UserId == userId &&
                    u.IsActive);

            if (user == null)
            {
                return NotFound("User not found or is not active.");
            }


            // 3. Check-in cannot be in the past
            var today = DateOnly.FromDateTime(DateTime.Now);

            if (dto.CheckInDate < today)
            {
                return BadRequest(
                    "Check-in date cannot be in the past.");
            }


            // 4. Check-out must be after Check-in
            if (dto.CheckOutDate <= dto.CheckInDate)
            {
                return BadRequest(
                    "Check-out date must be after Check-in date.");
            }


            // 5. Check GuestsCount
            if (dto.GuestsCount <= 0)
            {
                return BadRequest(
                    "Guests count must be greater than zero.");
            }


            // 6. Check Hotel exists
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.HotelId == dto.HotelId);

            if (hotel == null)
            {
                return NotFound("Hotel not found.");
            }


            // 7. Check RoomType exists
            var roomType = await _context.RoomTypes
                .FirstOrDefaultAsync(rt =>
                    rt.RoomTypeId == dto.RoomTypeId);

            if (roomType == null)
            {
                return NotFound("Room type not found.");
            }


            // 8. Check RoomType belongs to the selected Hotel
            var room = await _context.Rooms
                .Where(r =>
                    r.HotelId == dto.HotelId &&
                    r.RoomTypeId == dto.RoomTypeId &&
                    r.IsActive &&
                    !r.Reservations.Any(res =>
                        res.Status != "Cancelled" &&
                        dto.CheckInDate < res.CheckOutDate &&
                        dto.CheckOutDate > res.CheckInDate))
                .FirstOrDefaultAsync();

            if (room == null)
            {
                return Conflict(
                    "No available room of this type in the selected hotel for the selected dates.");
            }


            // 9. Check GuestsCount against RoomType capacity
            if (dto.GuestsCount > roomType.MaxGuests)
            {
                return BadRequest(
                    $"This room type can accommodate a maximum of {roomType.MaxGuests} guests.");
            }


            // 10. Calculate number of nights
            var numberOfNights =
                dto.CheckOutDate.DayNumber -
                dto.CheckInDate.DayNumber;


            // 11. Calculate total amount
            var totalAmount =
                numberOfNights * room.PricePerNight;


            // 12. Create Reservation
            var reservation = new Reservation
            {
                UserId = userId,
                RoomId = room.RoomId,
                CheckInDate = dto.CheckInDate,
                CheckOutDate = dto.CheckOutDate,
                GuestsCount = dto.GuestsCount,
                Status = "Pending",
                TotalAmount = totalAmount,
                CreatedAt = DateTime.UtcNow,
                CancelledAt = null
            };

            _context.Reservations.Add(reservation);

            await _context.SaveChangesAsync();


            // 13. Return 201 Created
            return CreatedAtAction(
                nameof(GetReservationById),
                new { id = reservation.ReservationId },
                new
                {
                    reservation.ReservationId,
                    reservation.UserId,
                    reservation.RoomId,
                    HotelId = dto.HotelId,
                    RoomTypeId = dto.RoomTypeId,
                    reservation.CheckInDate,
                    reservation.CheckOutDate,
                    reservation.GuestsCount,
                    reservation.Status,
                    reservation.TotalAmount,
                    reservation.CreatedAt
                }
            );
        }


        [HttpPut("UpdateReservation/{id}")]
        [Authorize(Policy = "UpdateReservation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateReservation(
        int id,
        UpdateReservationDto dto)
        {
            // 1. Get reservation
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound("Reservation not found.");
            }

            // 2. Cannot update cancelled reservation
            if (reservation.Status == "Cancelled")
            {
                return BadRequest("Cancelled reservation cannot be updated.");
            }

            // 3. Check-in cannot be in the past
            var today = DateOnly.FromDateTime(DateTime.Now);

            if (dto.CheckInDate < today)
            {
                return BadRequest("Check-in date cannot be in the past.");
            }

            // 4. Check-out must be after Check-in
            if (dto.CheckOutDate <= dto.CheckInDate)
            {
                return BadRequest(
                    "Check-out date must be after Check-in date.");
            }

            // 5. Check GuestsCount
            if (dto.GuestsCount <= 0)
            {
                return BadRequest(
                    "Guests count must be greater than zero.");
            }

            // 6. Check RoomType exists
            var roomType = await _context.RoomTypes
                .FirstOrDefaultAsync(rt =>
                    rt.RoomTypeId == dto.RoomTypeId);

            if (roomType == null)
            {
                return NotFound("Room type not found.");
            }

            // 7. Check GuestsCount against RoomType capacity
            if (dto.GuestsCount > roomType.MaxGuests)
            {
                return BadRequest(
                    $"This room type can accommodate a maximum of {roomType.MaxGuests} guests.");
            }

            // 8. Find an available room of the selected RoomType
            //    Ignore the current reservation itself
            var room = await _context.Rooms
                .Where(r =>
                    r.RoomTypeId == dto.RoomTypeId &&
                    r.IsActive &&
                    !r.Reservations.Any(res =>
                        res.ReservationId != id &&
                        res.Status != "Cancelled" &&
                        dto.CheckInDate < res.CheckOutDate &&
                        dto.CheckOutDate > res.CheckInDate))
                .FirstOrDefaultAsync();

            if (room == null)
            {
                return Conflict(
                    "No available room of this type for the selected dates.");
            }

            // 9. Calculate number of nights
            var numberOfNights =
                dto.CheckOutDate.DayNumber -
                dto.CheckInDate.DayNumber;

            // 10. Calculate total amount
            var totalAmount =
                numberOfNights * room.PricePerNight;

            // 11. Update reservation
            reservation.RoomId = room.RoomId;
            reservation.CheckInDate = dto.CheckInDate;
            reservation.CheckOutDate = dto.CheckOutDate;
            reservation.GuestsCount = dto.GuestsCount;
            reservation.TotalAmount = totalAmount;

            await _context.SaveChangesAsync();

            // 12. Return updated reservation
            return Ok(new
            {
                reservation.ReservationId,
                reservation.UserId,
                reservation.RoomId,
                RoomTypeId = dto.RoomTypeId,
                reservation.CheckInDate,
                reservation.CheckOutDate,
                reservation.GuestsCount,
                reservation.Status,
                reservation.TotalAmount,
                reservation.CreatedAt,
                reservation.CancelledAt
            });
        }

        [HttpDelete("CancelReservation/{id}")]
        [Authorize(Policy = "CancelReservation")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound("Reservation not found.");
            }

            if (reservation.Status == "Cancelled")
            {
                return BadRequest("Reservation is already cancelled.");
            }

            reservation.Status = "Cancelled";
            reservation.CancelledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
