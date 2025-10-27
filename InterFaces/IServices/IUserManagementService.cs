using automobile_backend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IUserManagementService
    {
        Task<UserDetailDto> AddEmployeeAsync(UserRegisterDto dto);
        Task<UserDetailDto?> GetUserByIdAsync(int id);
        Task<IEnumerable<UserSummaryDto>> GetAllUsersAsync();
        Task<UserDetailDto?> UpdateUserAsync(int id, UserUpdateDto dto);
        Task<bool> ActivateUserAsync(int id);
        Task<bool> DeactivateUserAsync(int id);
    }
}
