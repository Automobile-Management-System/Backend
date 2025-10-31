using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automobile_backend.Models.Entities
{
    public class ModificationRequest
    {
        [Key]
        public int ModificationId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;


        // Foreign Key for Appointment
        public int AppointmentId { get; set; }

        // Navigation property to the Appointment
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }

        public ModificationStatus Status { get; set; } = ModificationStatus.Pending;
    }
}