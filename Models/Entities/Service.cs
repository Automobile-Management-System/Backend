using System.ComponentModel.DataAnnotations.Schema; // <-- ADD THIS
using System.ComponentModel.DataAnnotations;

namespace automobile_backend.Models.Entities
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }
        
        public string ServiceName { get; set; }
        
        public string Description { get; set; }
        
        // ... other properties ...

        // --- ADD THIS PROPERTY ---
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        // Navigation property
        public ICollection<AppointmentService> AppointmentServices { get; set; }
    }
}