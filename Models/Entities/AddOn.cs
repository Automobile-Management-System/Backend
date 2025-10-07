using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automobile_backend.Models.Entities
{
    public class AddOn
    {
        [Key]
        public int AddOnId { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        // Foreign Key for Payment
        public int PaymentId { get; set; }

        // Navigation property to the Payment
        [ForeignKey("PaymentId")]
        public Payment Payment { get; set; }
    }
}