namespace automobile_backend.Models.DTOs
{
    public class ServiceCompletionDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public double CompletionRate { get; set; }
    }
}