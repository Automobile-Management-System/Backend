using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.Interfaces.IRepositories
{
    public interface ICustomerDashboardRepository
    {
        Task AddVehicleAsync(CustomerVehicle vehicle);
        Task<List<CustomerVehicle>> GetVehiclesByUserIdAsync(int userId);
    }
}
