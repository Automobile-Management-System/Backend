using automobile_backend.InterFaces.IServices;
using automobile_backend.InterFaces.IRepository;

namespace automobile_backend.Services
{
    public class EmployeeDashboardService : IEmployeeDashboardService
    {
        private readonly IEmployeeDashboardRepository _repository;

        public EmployeeDashboardService(IEmployeeDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> GetTodayUpcomingAppointmentCountAsync(int employeeId)
        {
            return await _repository.GetTodayUpcomingAppointmentCountAsync(employeeId);
        }

        public async Task<int> GetInProgressAppointmentCountAsync(int employeeId)
        {
            return await _repository.GetInProgressAppointmentCountAsync(employeeId);
        }

        public async Task<List<object>> GetTodayRecentServicesAsync(int employeeId)
        {
            return await _repository.GetTodayRecentServicesAsync(employeeId);
        }

        public async Task<List<object>> GetTodayRecentModificationsAsync(int employeeId)
        {
            return await _repository.GetTodayRecentModificationsAsync(employeeId);
        }


    }
}
