using automobile_backend.Models.DTOs;

namespace automobile_backend.Services.Interfaces
{
    public interface ICustomerVehicleService
    {
        Task<IEnumerable<ViewVehicleDto>> GetUserVehiclesAsync(int userId);
        Task<ViewVehicleDto?> GetVehicleByIdAsync(int vehicleId, int userId);
        Task<bool> AddVehicleAsync(int userId, CreateVehicleDto dto);
        Task<bool> UpdateVehicleAsync(int vehicleId, int userId, UpdateVehicleDto dto);
        Task<bool> DeleteVehicleAsync(int vehicleId, int userId);
    }
}
