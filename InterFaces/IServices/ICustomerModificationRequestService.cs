using automobile_backend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface ICustomerModificationRequestService
    {
        Task<IEnumerable<CustomerModificationRequestDto>> GetAllModificationRequestsAsync();
        Task<CustomerModificationRequestDto> AddModificationRequestAsync(CustomerModificationRequestDto dto); // ✅ return DTO
        Task<IEnumerable<CustomerModificationRequestDto>> GetByUserIdAsync(int userId);
    }
}
