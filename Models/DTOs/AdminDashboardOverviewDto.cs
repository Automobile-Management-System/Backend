namespace automobile_backend.Models.DTOs
{
    public class AdminDashboardOverviewDto
    {
        public decimal TotalRevenue { get; set; }
        public string Currency { get; set; } = "LKR"; // Added currency
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalAppointments { get; set; }
    }
}