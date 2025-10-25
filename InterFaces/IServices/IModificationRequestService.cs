using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IModificationRequestService
    {
        Task<IEnumerable<object>> GetAllModificationRequestsAsync();
        Task<object?> ReviewModificationRequestAsync(int id, ReviewRequestDto reviewDto);
    }
}