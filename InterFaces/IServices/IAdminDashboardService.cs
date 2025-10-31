using automobile_backend.Models.DTOs;

namespace automobile_backend.InterFaces.IServices
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardDto> GetDashboardDataAsync();
        Task<AdminDashboardStatsDto> GetDashboardStatsAsync();
        Task<AdminDashboardChartsDto> GetDashboardChartsAsync();
        Task<List<RecentUserDto>> GetRecentUsersAsync(int limit = 10);
        Task<List<SystemAlertDto>> GetSystemAlertsAsync();
        Task<bool> MarkAlertAsReadAsync(int alertId);
    }
}