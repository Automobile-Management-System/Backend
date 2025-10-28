using automobile_backend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IModificationRequestService
    {
        Task<IEnumerable<ModificationRequestDto>> GetAllModificationRequestsAsync();
        Task AddModificationRequestAsync(ModificationRequestDto modificationRequestDto);
    }
}
