using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IRepository
{
    public interface ICustomerModificationRequestRepository
    {
        // Get all modification requests (including related Appointment)
        Task<IEnumerable<ModificationRequest>> GetAllAsync();

        // Add a new modification request (appointment is created in service)
        Task AddAsync(ModificationRequest modificationRequest);

        // Get modification requests for a specific user (including related Appointment)
        Task<IEnumerable<ModificationRequest>> GetByUserIdAsync(int userId);
    }
}
