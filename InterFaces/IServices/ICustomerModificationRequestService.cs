using automobile_backend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface ICustomerModificationRequestService
    {
        Task<IEnumerable<CustomerModificationRequestDto>> GetAllModificationRequestsAsync();
        Task AddModificationRequestAsync(CustomerModificationRequestDto customermodificationRequestDto);
        Task<IEnumerable<CustomerModificationRequestDto>> GetByUserIdAsync(int userId);
    }
}
