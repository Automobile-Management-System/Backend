//using automobile_backend.Models.DTOs;
//using automobile_backend.Models.Entities;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace automobile_backend.InterFaces.IRepositories
//{
//    public interface IUserManagementRepository
//    {
//        Task<User> AddEmployeeAsync(UserRegisterDto dto);
//        Task<User?> GetUserByIdAsync(int id);
//        Task<IEnumerable<User>> GetAllUsersAsync();
//        Task<User?> UpdateUserAsync(int id, UserUpdateDto dto);
//        Task<bool> ActivateUserAsync(int id);
//        Task<bool> DeactivateUserAsync(int id);
//    }
//}

using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IRepositories
{
    public interface IUserManagementRepository
    {
        Task<User> AddEmployeeAsync(UserRegisterDto dto);
        Task<User?> GetUserByIdAsync(int id);
        Task<PagedResult<User>> GetUsersAsync(string? search, Enums? role, int pageNumber, int pageSize);
        Task<User?> UpdateUserAsync(int id, UserUpdateDto dto);
        Task<bool> ActivateUserAsync(int id);
        Task<bool> DeactivateUserAsync(int id);
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveUsersCountAsync();
        Task<int> GetActiveCustomersCountAsync();
        Task<int> GetActiveEmployeesCountAsync();
    }
}
