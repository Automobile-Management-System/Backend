using automobile_backend.Models.DTOs;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IAdminDashboardRepository
    {
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetTotalUsersCountForPreviousMonthAsync();
        Task<int> GetActiveBookingsCountAsync();
        Task<int> GetActiveBookingsCountForPreviousMonthAsync();
        Task<decimal> GetMonthlyRevenueAsync();
        Task<decimal> GetPreviousMonthRevenueAsync();
        Task<List<WeeklyRevenueDto>> GetWeeklyRevenueAsync();
        Task<List<WeeklyAppointmentDto>> GetWeeklyAppointmentsAsync();
        Task<List<RecentUserDto>> GetRecentUsersAsync(int limit);
        Task<List<SystemAlertDto>> GetSystemAlertsAsync();
        Task<bool> MarkAlertAsReadAsync(int alertId);
    }
}