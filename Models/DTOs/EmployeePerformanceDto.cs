namespace automobile_backend.Models.DTOs
{
    public class EmployeePerformanceDto
    {
        public string EmployeeName { get; set; } = string.Empty;
        public int CompletedAppointments { get; set; } // Changed from AppointmentsHandled
        public decimal RevenueGenerated { get; set; } // Revenue instead of hours logged
        public decimal AverageRating { get; set; }
    }
}