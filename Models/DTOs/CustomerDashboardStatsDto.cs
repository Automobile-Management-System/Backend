namespace automobile_backend.Models.DTOs
{
    public class CustomerDashboardStatsDto
    {
        public int UpcomingCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public decimal PendingPaymentsTotal { get; set; }
    }
}
