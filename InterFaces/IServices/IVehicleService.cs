using automobile_backend.Models.DTOs;

namespace automobile_backend.InterFaces.IServices
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleDto>> GetVehiclesByUserIdAsync(int userId);

    }
}
