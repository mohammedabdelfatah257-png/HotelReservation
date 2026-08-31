using HotelReservation.DTOs;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly HotelReservationDbContext _context;

        public ReviewController(HotelReservationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // Create Review
        // =========================================================

        [HttpPost("CreateReview")]
        [Authorize(Policy = "CreateReview")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateReview(AddReviewDto dto)
        {
            // 1. Get current user from JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            // 2. Validate Rating
            if (dto.Rating < 1 || dto.Rating > 5)
            {
                return BadRequest(
                    "Rating must be between 1 and 5.");
            }

            // 3. Get reservation belonging to current user
            var reservation = await _context.Reservations
                .Include(r => r.Room)
                .FirstOrDefaultAsync(r =>
                    r.ReservationId == dto.ReservationId &&
                    r.UserId == userId);

            if (reservation == null)
            {
                return NotFound(
                    "Reservation not found or does not belong to the current user.");
            }

            // 4. Reservation must be completed
            var today = DateOnly.FromDateTime(DateTime.Now);

            if (today < reservation.CheckOutDate)
            {
                return BadRequest(
                    "You can only review after your reservation has ended.");
            }
            // 5. Get HotelId from Room
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r =>
                    r.RoomId == reservation.RoomId);

            if (room == null)
            {
                return NotFound("Room not found.");
            }

            // 6. Check if user already reviewed this reservation
            var reviewExists = await _context.Reviews
                .AnyAsync(r =>
                    r.ReservationId == dto.ReservationId &&
                    r.UserId == userId);

            if (reviewExists)
            {
                return Conflict(
                    "You have already reviewed this reservation.");
            }

            // 7. Create review
            var review = new Review
            {
                UserId = userId,
                HotelId = room.HotelId,
                ReservationId = dto.ReservationId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);

            await _context.SaveChangesAsync();

            // 8. Return 201 Created
            return CreatedAtAction(
                nameof(GetReviewById),
                new { id = review.ReviewId },
                new
                {
                    review.ReviewId,
                    review.UserId,
                    review.HotelId,
                    review.ReservationId,
                    review.Rating,
                    review.Comment,
                    review.CreatedAt
                });
        }

        // =========================================================
        // Get Review By Id
        // =========================================================

        [HttpGet("ReviewById/{id}", Name = "GetReviewById")]
        [Authorize(Policy = "ViewReviews")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == id);

            if (review == null)
            {
                return NotFound("Review not found.");
            }

            return Ok(new
            {
                review.ReviewId,
                review.UserId,
                review.HotelId,
                review.ReservationId,
                review.Rating,
                review.Comment,
                review.CreatedAt
            });
        }



        [HttpGet("HotelReviews/{hotelId}")]
        [Authorize(Policy = "ViewReviews")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHotelReviews(int hotelId)
        {
            var hotelExists = await _context.Hotels
                .AnyAsync(h => h.HotelId == hotelId);

            if (!hotelExists)
            {
                return NotFound("Hotel not found.");
            }

            var reviews = await _context.Reviews
                .Where(r => r.HotelId == hotelId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.ReviewId,
                    r.ReservationId,
                    UserName = r.User.FirstName + " " + r.User.LastName,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

    }
}

