using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IModificationRequestService
    {
        Task<(IEnumerable<object> Data, int TotalCount)> GetAllModificationRequestsAsync(int pageNumber = 1, int pageSize = 10);
        Task<object?> ReviewModificationRequestAsync(int id, ReviewRequestDto reviewDto);
    }
}