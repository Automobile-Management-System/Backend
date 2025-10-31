using automobile_backend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IRepositories
{
    public interface IVehicleRepository
    {
        Task<IEnumerable<VehicleDto>> GetVehiclesByUserIdAsync(int userId);
    }
}
