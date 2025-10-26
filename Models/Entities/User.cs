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

        public byte[]? PasswordHash { get; set; }

        public byte[]? PasswordSalt { get; set; }

        public string? Address { get; set; }

        public string? ProfilePicture { get; set; }

        public string? Status { get; set; }

        [Required]
        public Enums Role { get; set; }


        // Navigation properties for relationships
        public ICollection<CustomerVehicle> CustomerVehicles { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<TimeLog> TimeLogs { get; set; }
        public ICollection<Report> Reports { get; set; }
        public ICollection<EmployeeAppointment> EmployeeAppointments { get; set; }
    }
}
