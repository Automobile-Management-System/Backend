using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.Interfaces.IRepositories
{
    public interface ICustomerDashboardRepository
    {
        Task AddVehicleAsync(CustomerVehicle vehicle);
        Task<List<CustomerVehicle>> GetVehiclesByUserIdAsync(int userId);

        // Dashboard stats
        Task<int> GetUpcomingAppointmentsCountAsync(int userId);
        Task<int> GetInProgressAppointmentsCountAsync(int userId);
        Task<int> GetCompletedAppointmentsCountAsync(int userId);
        Task<decimal> GetPendingPaymentsTotalAsync(int userId);
        Task<List<object>> GetLatestServicesAsync(int userId);
        Task<List<object>> GetLatestModificationsAsync(int userId);
    }
}
