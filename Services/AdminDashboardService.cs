using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;

namespace automobile_backend.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IAdminDashboardRepository _repository;

        public AdminDashboardService(IAdminDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<AdminDashboardDto> GetDashboardDataAsync()
        {
            var stats = await GetDashboardStatsAsync();
            var charts = await GetDashboardChartsAsync();
            var recentUsers = await GetRecentUsersAsync();
            var systemAlerts = await GetSystemAlertsAsync();

            return new AdminDashboardDto
            {
                Stats = stats,
                Charts = charts,
                RecentUsers = recentUsers,
                SystemAlerts = systemAlerts
            };
        }

        public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync()
        {
            var totalUsers = await _repository.GetTotalUsersCountAsync();
            var previousMonthUsers = await _repository.GetTotalUsersCountForPreviousMonthAsync();
            var totalUsersChange = totalUsers - previousMonthUsers;

            var activeBookings = await _repository.GetActiveBookingsCountAsync();
            var previousMonthBookings = await _repository.GetActiveBookingsCountForPreviousMonthAsync();
            var activeBookingsChange = activeBookings - previousMonthBookings;

            var monthlyRevenue = await _repository.GetMonthlyRevenueAsync();
            var previousMonthRevenue = await _repository.GetPreviousMonthRevenueAsync();
            var monthlyRevenueChange = monthlyRevenue - previousMonthRevenue;

            // Calculate growth rate as percentage change in revenue
            var growthRate = previousMonthRevenue > 0 
                ? ((monthlyRevenue - previousMonthRevenue) / previousMonthRevenue) * 100 
                : 0;

            // Mock previous growth rate for comparison
            var previousGrowthRate = 18.2m; // This would be calculated from historical data
            var growthRateChange = growthRate - previousGrowthRate;

            return new AdminDashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalUsersChangeFromLastMonth = totalUsersChange,
                ActiveBookings = activeBookings,
                ActiveBookingsChangeFromLastMonth = activeBookingsChange,
                MonthlyRevenue = monthlyRevenue,
                MonthlyRevenueChangeFromLastMonth = monthlyRevenueChange,
                GrowthRate = growthRate,
                GrowthRateChangeFromLastMonth = growthRateChange
            };
        }

        public async Task<AdminDashboardChartsDto> GetDashboardChartsAsync()
        {
            var weeklyRevenue = await _repository.GetWeeklyRevenueAsync();
            var weeklyAppointments = await _repository.GetWeeklyAppointmentsAsync();

            return new AdminDashboardChartsDto
            {
                WeeklyRevenue = weeklyRevenue,
                WeeklyAppointments = weeklyAppointments
            };
        }

        public async Task<List<RecentUserDto>> GetRecentUsersAsync(int limit = 10)
        {
            return await _repository.GetRecentUsersAsync(limit);
        }

        public async Task<List<SystemAlertDto>> GetSystemAlertsAsync()
        {
            return await _repository.GetSystemAlertsAsync();
        }

        public async Task<bool> MarkAlertAsReadAsync(int alertId)
        {
            return await _repository.MarkAlertAsReadAsync(alertId);
        }
    }
}