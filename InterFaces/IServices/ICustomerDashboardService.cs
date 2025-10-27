using automobile_backend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.Interfaces.IServices
{
    public interface ICustomerDashboardService
    {
        Task AddVehicleAsync(int userId, CustomerVehicleDto dto);
        Task<List<CustomerVehicleDto>> GetVehiclesByUserIdAsync(int userId);
    }
}
