using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automobile_backend.Models.Entities
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BasePrice { get; set; }

        // Navigation property for the many-to-many relationship
        // Make it nullable with ? to avoid validation issues
        public ICollection<AppointmentService>? AppointmentServices { get; set; }
    }
}