using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IModificationRequestRepository
    {
        Task<IEnumerable<ModificationRequest>> GetAllAsync();
    }
}
