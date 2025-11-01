using automobile_backend.Models.Entities;

namespace automobile_backend.Repositories.Interfaces
{
    public interface ICustomerVehicleRepository
    {
        Task<IEnumerable<CustomerVehicle>> GetVehiclesByUserIdAsync(int userId);
        Task<CustomerVehicle?> GetVehicleByIdAsync(int vehicleId, int userId);
        Task AddVehicleAsync(CustomerVehicle vehicle);
        Task UpdateVehicleAsync(CustomerVehicle vehicle);
        Task DeleteVehicleAsync(CustomerVehicle vehicle);
        Task SaveChangesAsync();
    }
}
