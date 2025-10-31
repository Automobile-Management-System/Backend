using System.Collections.Generic;

namespace automobile_backend.Models.DTOs
{
    public class AnalyticsOverviewDto
    {
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalEmployees { get; set; }
    }
}