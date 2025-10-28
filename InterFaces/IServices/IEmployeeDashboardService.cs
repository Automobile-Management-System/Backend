using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IEmployeeDashboardService
    {
        Task<int> GetTodayUpcomingAppointmentCountAsync(int employeeId);

        Task<int> GetInProgressAppointmentCountAsync(int employeeId);

        Task<List<object>> GetTodayRecentServicesAsync(int employeeId);

        Task<List<object>> GetTodayRecentModificationsAsync(int employeeId);

    }
}
