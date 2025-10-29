using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IRepository
{
    public interface ICustomerModificationRequestRepository
    {
        Task<IEnumerable<ModificationRequest>> GetAllAsync();
        Task AddAsync(ModificationRequest modificationRequest);
    }
}
