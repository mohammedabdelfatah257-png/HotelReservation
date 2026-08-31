namespace HotelReservation.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public int UserId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public List<string> Permissions { get; set; } = new();

    }

    public class AddHotelDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? Country { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }
        public byte StarRating { get; set; }
    }

    public class UpdateHotelDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? Country { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public byte StarRating { get; set; }

        public bool IsActive { get; set; }
    }

    public class AddRoomDto
    {
        public int HotelId { get; set; }

        public int RoomTypeId { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        public int Floor { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal PricePerNight { get; set; }

         
    }

    public class UpdateRoomDto
    {
        public int HotelId { get; set; }

        public int RoomTypeId { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        public int Floor { get; set; }

        public string Status { get; set; } = string.Empty;

        public decimal PricePerNight { get; set; }

        public bool IsActive { get; set; }
    }

    public class AddReservationDto
    {
        public int HotelId { get; set; }

        public int RoomTypeId { get; set; }

        public DateOnly CheckInDate { get; set; }

        public DateOnly CheckOutDate { get; set; }

        public int GuestsCount { get; set; }
    }


    public class UpdateReservationDto
    {
        public int RoomTypeId { get; set; }

        public DateOnly CheckInDate { get; set; }

        public DateOnly CheckOutDate { get; set; }

        public int GuestsCount { get; set; }
    }

    public class AddPaymentDto
    {
        public int ReservationId { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; } = null!;

    }

    public class AddReviewDto
    {
        public int ReservationId { get; set; } 
        public byte Rating { get; set; } 
        public string? Comment { get; set; }
    }

    public class AddUserDto {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!; 
        public string? Phone { get; set; }
        public string RoleName { get; set; } = null!;
    }

    public class RegisterDto
    { 
        public string FirstName { get; set; } = null!; 
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Phone { get; set; } 
    }

    public class UpdateMyProfileDto
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }
    }

}