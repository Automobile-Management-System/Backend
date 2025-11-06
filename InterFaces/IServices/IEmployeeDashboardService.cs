using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IEmployeeDashboardService
    {
        Task<int> GetUpcomingAppointmentCountAsync(int employeeId);

        Task<int> GetInProgressAppointmentCountAsync(int employeeId);

        Task<List<object>> GetRecentServicesAsync(int employeeId);

        Task<List<object>> GetRecentModificationsAsync(int employeeId);
        Task<int> GetCompletedServiceCountAsync(int employeeId);
        Task<int> GetCompletedModificationCountAsync(int employeeId);


    }
}
