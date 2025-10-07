using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automobile_backend.Models.Entities
{
    public class TimeLog
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal HoursLogged { get; set; }

        public string? Notes { get; set; }

        // Foreign Key for Appointment
        public int AppointmentId { get; set; }

        // Navigation property to the Appointment
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }

        // Foreign Key for the User who logged the time
        public int UserId { get; set; }

        // Navigation property to the User
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}