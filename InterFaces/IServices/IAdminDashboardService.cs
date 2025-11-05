using automobile_backend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardOverviewDto> GetDashboardOverviewAsync();
        Task<WeeklyRevenueDto> GetWeeklyRevenueAsync();
        Task<WeeklyAppointmentsDto> GetWeeklyAppointmentsAsync();
        Task<IEnumerable<RecentUserDto>> GetRecentUsersAsync(int count = 10);
    }
}