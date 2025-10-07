using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace automobile_backend.Models.Entities
{
    public class CustomerVehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [Required]
        [MaxLength(20)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required]
        public FuelType FuelType { get; set; }

        [Required]
        [MaxLength(50)]
        public string ChassisNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        // Foreign Key for User
        public int UserId { get; set; }

        // Navigation property to the User
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
