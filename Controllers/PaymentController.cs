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
    public class PaymentController : ControllerBase
    {

        private readonly HotelReservationDbContext _context;

        public PaymentController(HotelReservationDbContext context)
        {

            _context = context;
        }

        [HttpPost("AddPayment")]
        [Authorize(Policy = "CreatePayment")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AddPayment(AddPaymentDto dto)
        {
            // 1. Get current user from JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            // 2. Check reservation exists and belongs to current user
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r =>
                    r.ReservationId == dto.ReservationId &&
                    r.UserId == userId);

            if (reservation == null)
            {
                return NotFound(
                    "Reservation not found or does not belong to the current user.");
            }

            // 3. Cannot pay for cancelled reservation
            if (reservation.Status == "Cancelled")
            {
                return BadRequest(
                    "Cannot make a payment for a cancelled reservation.");
            }

            // 4. Check amount
            if (dto.Amount <= 0)
            {
                return BadRequest(
                    "Payment amount must be greater than zero.");
            }

            // 5. Check payment method
            if (string.IsNullOrWhiteSpace(dto.PaymentMethod))
            {
                return BadRequest(
                    "Payment method is required.");
            }

            // 6. Calculate previous payments
            var previousPayments = await _context.Payments
                .Where(p =>
                    p.ReservationId == dto.ReservationId &&
                    p.PaymentStatus == "Paid")
                .SumAsync(p => p.Amount);

            // 7. Check if reservation is already fully paid
            if (previousPayments >= reservation.TotalAmount)
            {
                return Conflict(
                    "This reservation has already been fully paid.");
            }

            // 8. Check payment amount does not exceed remaining amount
            var remainingAmount =
                reservation.TotalAmount - previousPayments;

            if (dto.Amount > remainingAmount)
            {
                return BadRequest(
                    $"Payment amount cannot exceed the remaining amount: {remainingAmount}.");
            }

            // 9. Create payment
            var payment = new Payment
            {
                ReservationId = dto.ReservationId,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = "Paid",
                TransactionReference = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                PaidAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            // 10. Return 201 Created
            return CreatedAtAction(
                nameof(GetPaymentById),
                new { id = payment.PaymentId },
                new
                {
                    payment.PaymentId,
                    payment.ReservationId,
                    payment.Amount,
                    payment.PaymentMethod,
                    payment.PaymentStatus,
                    payment.TransactionReference,
                    payment.PaidAt
                }
            );
        }

        [HttpGet("PaymentById/{id}", Name = "GetPaymentById")]
        [Authorize(Policy = "ViewPayments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            // 1. Get current user from JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            // 2. Get payment and make sure it belongs to current user
            var payment = await _context.Payments
                .Include(p => p.Reservation)
                .FirstOrDefaultAsync(p =>
                    p.PaymentId == id &&
                    p.Reservation.UserId == userId);

            // 3. Payment not found or does not belong to current user
            if (payment == null)
            {
                return NotFound(
                    "Payment not found or does not belong to the current user.");
            }

            // 4. Return payment
            return Ok(new
            {
                payment.PaymentId,
                payment.ReservationId,
                payment.Amount,
                payment.PaymentMethod,
                payment.PaymentStatus,
                payment.TransactionReference,
                payment.PaidAt
            });
        }

        [HttpGet("AllPayments")]
        [Authorize(Policy = "ViewPayments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllPayments()
        {
            // 1. Get current user from JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            // 2. Get current user's role
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // 3. Get all payments
            var query = _context.Payments
                .AsNoTracking()
                .Include(p => p.Reservation)
                .AsQueryable();

            // 4. Customer sees only his own payments
            if (role == "Customer")
            {
                query = query.Where(p =>
                    p.Reservation.UserId == userId);
            }

            // 5. Select payment data
            var payments = await query
                .Select(p => new
                {
                    p.PaymentId,
                    p.ReservationId,
                    p.Amount,
                    p.PaymentMethod,
                    p.PaymentStatus,
                    p.TransactionReference,
                    p.PaidAt
                })
                .ToListAsync();

            // 6. Return result
            return Ok(payments);
        }

        [HttpGet("MyPayments")]
        [Authorize(Policy = "ViewPayments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyPayments()
        {
            // 1. Get current user from JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            // 2. Get payments that belong to the current user
            var payments = await _context.Payments
                .AsNoTracking()
                .Include(p => p.Reservation)
                .Where(p => p.Reservation.UserId == userId)
                .Select(p => new
                {
                    p.PaymentId,
                    p.ReservationId,
                    p.Amount,
                    p.PaymentMethod,
                    p.PaymentStatus,
                    p.TransactionReference,
                    p.PaidAt
                })
                .ToListAsync();

            // 3. Return payments
            return Ok(payments);
        }



    }
}
