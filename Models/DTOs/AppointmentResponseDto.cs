using automobile_backend.Models.Entities;

namespace automobile_backend.Models.DTOs
{
    public class AppointmentResponseDto
    {
        public int AppointmentId { get; set; }
        public DateTime DateTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }

        public int VehicleId { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;

        public List<ServiceDto> Services { get; set; } = new();
    }

    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal BasePrice { get; set; }
    }
}