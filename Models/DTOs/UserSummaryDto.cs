using automobile_backend.Models.Entities;

namespace automobile_backend.Models.DTOs
{
    public class UserSummaryDto
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? Role { get; set; }

        public string? Status { get; set; }
    }
}
