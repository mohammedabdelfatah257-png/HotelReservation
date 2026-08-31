using HotelReservation.DTOs;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private readonly HotelReservationDbContext _context;

        public HotelController(HotelReservationDbContext context)
        {
            _context = context;
        }

        [HttpGet("AllHotels")]
        [Authorize(Policy = "ViewHotels")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetHotels()
        {
            var hotels = await _context.Hotels.ToListAsync();

            return Ok(hotels);
        }

        [HttpGet("HotelById/{id}")]
        [Authorize(Policy = "ViewHotels")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHotelById(int id)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null)
            {
                return NotFound();
            }

            return Ok(hotel);
        }

        [HttpPost("AddHotel")]
        [Authorize(Policy = "CreateHotel")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddHotel(AddHotelDto dto)
        {
            var hotel = new Hotel
            {
                Name = dto.Name,
                Description = dto.Description,
                Address = dto.Address,
                City = dto.City,
                Country = dto.Country,
                Phone = dto.Phone,
                Email = dto.Email,
                StarRating = dto.StarRating,
                IsActive = true
            };

            _context.Hotels.Add(hotel);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetHotelById),
                new { id = hotel.HotelId },
                hotel
            );
        }

        [HttpPut("UpdateHotel/{id}")]
        [Authorize(Policy = "UpdateHotel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateHotel(
            int id,
            UpdateHotelDto dto)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null)
            {
                return NotFound();
            }

            hotel.Name = dto.Name;
            hotel.Description = dto.Description;
            hotel.Address = dto.Address;
            hotel.City = dto.City;
            hotel.Country = dto.Country;
            hotel.Phone = dto.Phone;
            hotel.Email = dto.Email;
            hotel.StarRating = dto.StarRating;
            hotel.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(hotel);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "DeleteHotel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (hotel == null)
            {
                return NotFound();
            }

            _context.Hotels.Remove(hotel);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}