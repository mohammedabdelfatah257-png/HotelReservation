using HotelReservation.DTOs;
using HotelReservation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HotelReservation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly HotelReservationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        public UserController(
           HotelReservationDbContext context,
           IPasswordHasher<User> passwordHasher,
           IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        
        [HttpPost("Register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // 1. Validate required fields

            if (string.IsNullOrWhiteSpace(dto.FirstName))
            {
                return BadRequest("First name is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.LastName))
            {
                return BadRequest("Last name is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Password is required.");
            }


            // 2. Check email already exists

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExists)
            {
                return Conflict("Email is already registered.");
            }


            // 3. Get Customer role

            var customerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Customer");

            if (customerRole == null)
            {
                return BadRequest("Customer role was not found.");
            }


            // 4. Create user

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Phone = dto.Phone,
                RoleId = customerRole.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };


            // 5. Hash password

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                dto.Password);


            // 6. Save user

            _context.Users.Add(user);

            await _context.SaveChangesAsync();


            // 7. Return result

            return Created(
                "",
                new
                {
                    user.UserId,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Phone,
                    Role = customerRole.Name,
                    user.IsActive,
                    user.CreatedAt
                });
        }

        [HttpGet("MyProfile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile()
        {
            // 1. Get current UserId from JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            // 2. Get current user
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // 3. Return user data
            return Ok(new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                user.IsActive,
                user.CreatedAt,
                Role = user.Role.Name
            });
        }

        [HttpPut("UpdateMyProfile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateMyProfile(UpdateMyProfileDto dto)
        {
            // 1. Get current UserId from JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            // 2. Get current user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // 3. Validate data
            if (string.IsNullOrWhiteSpace(dto.FirstName) ||
                string.IsNullOrWhiteSpace(dto.LastName) ||
                string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(
                    "First name, last name and email are required.");
            }

            // 4. Check email is not used by another user
            var emailExists = await _context.Users
                .AnyAsync(u =>
                    u.Email == dto.Email &&
                    u.UserId != userId);

            if (emailExists)
            {
                return Conflict("Email is already in use.");
            }

            // 5. Update user
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;

            await _context.SaveChangesAsync();

            // 6. Return updated data
            return Ok(new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                user.IsActive,
                user.CreatedAt
            });
        }

        [HttpDelete("DeactivateMyAccount")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateMyAccount()
        {
            // 1. Get current UserId from JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            // 2. Get current user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // 3. Check if already inactive
            if (!user.IsActive)
            {
                return BadRequest("Your account is already deactivated.");
            }

            // 4. Deactivate account
            user.IsActive = false;

            await _context.SaveChangesAsync();

            // 5. Return 204
            return NoContent();
        }

        [HttpGet("AllUsers")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Phone,
                    u.IsActive,
                    u.CreatedAt,
                    Role = u.Role.Name
                })
                .ToListAsync();

            if (users.Count == 0)
            {
                return NotFound("No users found.");
            }

            return Ok(users);
        }

        [HttpGet("UserById/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                user.IsActive,
                user.CreatedAt,
                Role = user.Role.Name
            });
        }
    }
}
