using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automobile_backend.Models.Entities
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public DateTime PaymentDateTime { get; set; }

        public string? InvoiceLink { get; set; }

        // Foreign Key for Appointment
        public int AppointmentId { get; set; }

        // Navigation property establishing the one-to-one link
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }

        // Navigation property for related add-ons
        public ICollection<AddOn> AddOns { get; set; }
    }
}