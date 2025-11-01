
using automobile_backend.Models.Entities; 
using System.ComponentModel.DataAnnotations;

namespace automobile_backend.Models.DTOs
{
    public class UpdatePaymentStatusDto
    {
        [Required]
        // This ensures the incoming value is a valid member of the PaymentStatus enum
        public PaymentStatus Status { get; set; }
    }
}