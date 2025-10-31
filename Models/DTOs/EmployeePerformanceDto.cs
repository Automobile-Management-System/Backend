namespace automobile_backend.Models.DTOs
{
    public class EmployeePerformanceDto
    {
        public string EmployeeName { get; set; } = string.Empty;
        public int AppointmentsHandled { get; set; }
        public decimal AverageRating { get; set; }
        public decimal TotalHoursLogged { get; set; }
    }
}