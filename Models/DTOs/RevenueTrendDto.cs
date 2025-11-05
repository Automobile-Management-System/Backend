using System.Collections.Generic;

namespace automobile_backend.Models.DTOs
{
    public class RevenueTrendDto
    {
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, int> AppointmentsByMonth { get; set; } = new Dictionary<string, int>();
    }
}