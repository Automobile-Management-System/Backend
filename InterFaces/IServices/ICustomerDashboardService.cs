using System.Collections.Generic;
using System.Threading.Tasks;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;

namespace automobile_backend.Interfaces.IServices
{
    public interface ICustomerDashboardService
    {
        Task AddVehicleAsync(int userId, CreateVehicleDto dto);
        Task<List<CreateVehicleDto>> GetVehiclesByUserIdAsync(int userId);

        Task<int> GetUpcomingAppointmentsCountAsync(int userId);
        Task<int> GetInProgressAppointmentsCountAsync(int userId);
        Task<int> GetCompletedAppointmentsCountAsync(int userId);
        Task<decimal> GetPendingPaymentsTotalAsync(int userId);
        Task<List<object>> GetLatestServicesAsync(int userId);
        Task<List<object>> GetLatestModificationsAsync(int userId);
    }
}
