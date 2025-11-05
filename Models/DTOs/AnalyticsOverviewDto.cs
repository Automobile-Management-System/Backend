using System.Collections.Generic;

namespace automobile_backend.Models.DTOs
{
    public class AnalyticsOverviewDto
    {
        public decimal TotalRevenue { get; set; } // Year to date
        public string Currency { get; set; } = "LKR"; // Currency indicator
        public int TotalAppointments { get; set; } // Total bookings (not just completed)
        public decimal AverageRevenuePerMonth { get; set; } // Per month
        public double GrowthRate { get; set; } // Month over month percentage
    }
}