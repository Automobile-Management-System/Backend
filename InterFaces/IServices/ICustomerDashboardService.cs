using System.Collections.Generic;
using System.Threading.Tasks;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;

namespace automobile_backend.Interfaces.IServices
{
    public interface ICustomerDashboardService
    {
        Task AddVehicleAsync(int userId, CustomerVehicleDto dto);
        Task<List<CustomerVehicleDto>> GetVehiclesByUserIdAsync(int userId);

        Task<int> GetUpcomingAppointmentsCountAsync(int userId);
        Task<int> GetInProgressAppointmentsCountAsync(int userId);
        Task<int> GetCompletedAppointmentsCountAsync(int userId);
        Task<decimal> GetPendingPaymentsTotalAsync(int userId);
        Task<IEnumerable<Appointment>> GetLatestServicesAsync(int userId);
        Task<IEnumerable<Appointment>> GetLatestModificationsAsync(int userId);
    }
}
