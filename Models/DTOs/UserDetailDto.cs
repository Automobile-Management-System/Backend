namespace automobile_backend.Models.DTOs
{
    public class UserDetailDto
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? Status { get; set; }

        public string Role { get; set; } = string.Empty;
    }
}
