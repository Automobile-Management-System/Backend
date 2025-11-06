namespace automobile_backend.Models.DTOs
{
    public class StatusUpdateRequestDto
    {
        public string EntityType { get; set; } = "Appointment"; // e.g., "Appointment"
        public int EntityId { get; set; }
        public string NewStatus { get; set; } = string.Empty;    // e.g., "Completed"
    }
}