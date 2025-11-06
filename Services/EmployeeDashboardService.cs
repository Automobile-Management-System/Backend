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

        public async Task<int> GetUpcomingAppointmentCountAsync(int employeeId)
        {
            return await _repository.GetUpcomingAppointmentCountAsync(employeeId);
        }

        public async Task<int> GetInProgressAppointmentCountAsync(int employeeId)
        {
            return await _repository.GetInProgressAppointmentCountAsync(employeeId);
        }

        public async Task<List<object>> GetRecentServicesAsync(int employeeId)
        {
            return await _repository.GetRecentServicesAsync(employeeId);
        }

        public async Task<List<object>> GetRecentModificationsAsync(int employeeId)
        {
            return await _repository.GetRecentModificationsAsync(employeeId);
        }

        public async Task<int> GetCompletedServiceCountAsync(int employeeId)
        {
            return await _repository.GetCompletedServiceCountAsync(employeeId);
        }

        public async Task<int> GetCompletedModificationCountAsync(int employeeId)
        {
            return await _repository.GetCompletedModificationCountAsync(employeeId);
        }



    }
}
