using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automobile_backend.Models.Entities
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        [Range(1, 5)] // Assuming a 1-5 star rating system
        public int Rating { get; set; }

        public string? ReviewText { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        // Foreign Key for Appointment
        public int AppointmentId { get; set; }

        // Navigation property for the one-to-one relationship
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }
    }
}