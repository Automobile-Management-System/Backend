
namespace automobile_backend.Models.DTOs
{
    public class ProfileManagementDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Status { get; set; }
        
    }
}

namespace automobile_backend.Models.DTOs
{
    public class ProfileUpdateDto
    {
        
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? ProfilePicture { get; set; } // base64 or url per your app conventions
        public string? Status { get; set; }

        // Password update fields (plain text in request). If null/empty -> don't change.
        public string? NewPassword { get; set; }
    }
}
