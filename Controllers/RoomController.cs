using HotelReservation.DTOs;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly HotelReservationDbContext _context;

        public RoomController(HotelReservationDbContext context)
        {
            _context = context;
        }

        [HttpGet("AllRoom")]
        [Authorize(Policy = "ViewRooms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _context.Rooms
                .ToListAsync();

            if (rooms.Count == 0)
            {
                return NotFound("No rooms found.");
            }

            return Ok(rooms);
        }

        [HttpGet("RoomById/{id}")]
        [Authorize(Policy = "ViewRooms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null)
            {
                return NotFound();
            }

            return Ok(room);
        }

        [HttpPost("AddRoom")]
        [Authorize(Policy = "CreateRoom")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddRoom(AddRoomDto dto)
        {
            var room = new Room
            {
                HotelId = dto.HotelId,
                RoomTypeId = dto.RoomTypeId,
                RoomNumber = dto.RoomNumber,
                Floor = dto.Floor,
                Status = dto.Status,
                PricePerNight = dto.PricePerNight,
                IsActive = true
            };

            _context.Rooms.Add(room);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetRoomById),
                new { id = room.RoomId },
                room
            );
        }

        [HttpPut("UpdateRoom/{id}")]
        [Authorize(Policy = "UpdateRoom")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateRoom( int id, UpdateRoomDto dto)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null)
            {
                return NotFound();
            }

            room.HotelId = dto.HotelId;
            room.RoomTypeId = dto.RoomTypeId;
            room.RoomNumber = dto.RoomNumber;
            room.Floor = dto.Floor;
            room.Status = dto.Status;
            room.PricePerNight = dto.PricePerNight;
            room.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(room);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "DeleteRoom")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null)
            {
                return NotFound();
            }

            _context.Rooms.Remove(room);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
