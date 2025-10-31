namespace automobile_backend.Models.DTOs
{
    public class AdminDashboardDto
    {
        public AdminDashboardStatsDto Stats { get; set; } = new();
        public AdminDashboardChartsDto Charts { get; set; } = new();
        public List<RecentUserDto> RecentUsers { get; set; } = new();
        public List<SystemAlertDto> SystemAlerts { get; set; } = new();
    }

    public class AdminDashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalUsersChangeFromLastMonth { get; set; }
        public int ActiveBookings { get; set; }
        public int ActiveBookingsChangeFromLastMonth { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal MonthlyRevenueChangeFromLastMonth { get; set; }
        public decimal GrowthRate { get; set; }
        public decimal GrowthRateChangeFromLastMonth { get; set; }
    }

    public class AdminDashboardChartsDto
    {
        public List<WeeklyRevenueDto> WeeklyRevenue { get; set; } = new();
        public List<WeeklyAppointmentDto> WeeklyAppointments { get; set; } = new();
    }

    public class WeeklyRevenueDto
    {
        public string Day { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class WeeklyAppointmentDto
    {
        public string Day { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
    }

    public class RecentUserDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    public class SystemAlertDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // warning, info, success
        public string Message { get; set; } = string.Empty;
        public string ActionText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}