namespace automobile_backend.Models.DTOs
{
    public class CreateVehicleDto
    {
        public string VehicleId { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty; // Enum as string
        public string ChassisNumber { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
    }
}
