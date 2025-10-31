using System.Collections.Generic;

namespace automobile_backend.Models.DTOs
{
    public class RevenueStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new Dictionary<string, decimal>();
    }
}