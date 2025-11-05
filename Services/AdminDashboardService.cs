using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IAdminDashboardRepository _repository;

        public AdminDashboardService(IAdminDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<AdminDashboardOverviewDto> GetDashboardOverviewAsync()
        {
            return await _repository.GetDashboardOverviewAsync();
        }

        public async Task<WeeklyRevenueDto> GetWeeklyRevenueAsync()
        {
            return await _repository.GetWeeklyRevenueAsync();
        }

        public async Task<WeeklyAppointmentsDto> GetWeeklyAppointmentsAsync()
        {
            return await _repository.GetWeeklyAppointmentsAsync();
        }

        public async Task<IEnumerable<RecentUserDto>> GetRecentUsersAsync(int count = 10)
        {
            return await _repository.GetRecentUsersAsync(count);
        }
    }
}