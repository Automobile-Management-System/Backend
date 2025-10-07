namespace automobile_backend.Models.Entities
{
    using System.ComponentModel.DataAnnotations;

    public class User
    {
        [Key]
        public int UserId { get; set; } // Corresponds to User_ID

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; } = new byte[32]; // For HashPassword

        [Required]
        public byte[] PasswordSalt { get; set; } = new byte[32]; // Important for security

        public string? Address { get; set; }

        public string? ProfilePicture { get; set; }

        [Required]
        public Enums Role { get; set; }
    }
}
