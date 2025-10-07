using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automobile_backend.Models.Entities
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; }

        // Foreign Key for User
        public int UserId { get; set; }

        // Navigation property to the User
        [ForeignKey("UserId")]
        public User User { get; set; }

        // Navigation properties for related data
        public ICollection<AppointmentService> AppointmentServices { get; set; }
        public ICollection<ModificationRequest> ModificationRequests { get; set; }
        public ICollection<TimeLog> TimeLogs { get; set; }
        public Payment? Payment { get; set; } // An appointment can have one payment
        public Review? Review { get; set; }   // An appointment can have one review
    }
}