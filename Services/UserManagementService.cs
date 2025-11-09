// using automobile_backend.InterFaces.IRepositories;
// using automobile_backend.InterFaces.IServices;
// using automobile_backend.Models.DTOs;
// using automobile_backend.Models.Entities;

// namespace automobile_backend.Services
// {
//     public class UserManagementService : IUserManagementService
//     {
//         private readonly IUserManagementRepository _repo;

//         public UserManagementService(IUserManagementRepository repo)
//         {
//             _repo = repo;
//         }

//         public async Task<UserDetailDto> AddEmployeeAsync(UserRegisterDto dto)
//         {
//             var user = await _repo.AddEmployeeAsync(dto);
//             return MapToDetailDto(user);
//         }

//         public async Task<UserDetailDto?> GetUserByIdAsync(int id)
//         {
//             var user = await _repo.GetUserByIdAsync(id);
//             return user == null ? null : MapToDetailDto(user);
//         }

//         // Search + Filter + Pagination
//         public async Task<PagedResult<UserSummaryDto>> GetUsersAsync(string? search, string? role, int pageNumber, int pageSize)
//         {
//             Enums? parsedRole = null;

//             if (!string.IsNullOrEmpty(role) && Enum.TryParse<Enums>(role, true, out var parsed))
//             {
//                 parsedRole = parsed;
//             }

//             var result = await _repo.GetUsersAsync(search, parsedRole, pageNumber, pageSize);

//             return new PagedResult<UserSummaryDto>
//             {
//                 Items = result.Items.Select(u => new UserSummaryDto
//                 {
//                     UserId = u.UserId,
//                     FirstName = u.FirstName,
//                     LastName = u.LastName,
//                     Email = u.Email,
//                     PhoneNumber = u.PhoneNumber,
//                     Role = u.Role.ToString(),
//                     Status = u.Status
//                 }),
//                 TotalCount = result.TotalCount,
//                 PageNumber = result.PageNumber,
//                 PageSize = result.PageSize
//             };
//         }

//         public async Task<UserDetailDto?> UpdateUserAsync(int id, UserUpdateDto dto)
//         {
//             var user = await _repo.UpdateUserAsync(id, dto);
//             return user == null ? null : MapToDetailDto(user);
//         }

//         public async Task<bool> ActivateUserAsync(int id) => await _repo.ActivateUserAsync(id);

//         public async Task<bool> DeactivateUserAsync(int id) => await _repo.DeactivateUserAsync(id);

//         private static UserDetailDto MapToDetailDto(User user)
//         {
//             return new UserDetailDto
//             {
//                 UserId = user.UserId,
//                 FirstName = user.FirstName,
//                 LastName = user.LastName,
//                 Email = user.Email,
//                 PhoneNumber = user.PhoneNumber,
//                 Address = user.Address,
//                 Status = user.Status,
//                 Role = user.Role.ToString()
//             };
//         }
//         public async Task<int> GetTotalUsersCountAsync()
//         {
//             return await _repo.GetTotalUsersCountAsync();
//         }

//         public async Task<int> GetActiveUsersCountAsync()
//         {
//             return await _repo.GetActiveUsersCountAsync();
//         }

//         public async Task<int> GetActiveCustomersCountAsync()
//         {
//             return await _repo.GetActiveCustomersCountAsync();
//         }

//         public async Task<int> GetActiveEmployeesCountAsync()
//         {
//             return await _repo.GetActiveEmployeesCountAsync();
//         }
//     }
// }

//websocket after
using automobile_backend.InterFaces.IRepositories;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using automobile_backend.WebSockets; // ✅ Add this for notifications
using System.Text.Json;

namespace automobile_backend.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserManagementRepository _repo;

        public UserManagementService(IUserManagementRepository repo)
        {
            _repo = repo;
        }

        public async Task<UserDetailDto> AddEmployeeAsync(UserRegisterDto dto)
        {
            var user = await _repo.AddEmployeeAsync(dto);
            var userDto = MapToDetailDto(user);

            // ✅ Notify all admins in real-time
            await WebSocketHandler.BroadcastObjectAsync(new
            {
                type = "USER_ADDED",
                message = $"New employee added: {user.FirstName} {user.LastName}",
                name = user.FirstName + " " + user.LastName,
                email = user.Email,
                timestamp = DateTime.UtcNow.ToString("o")
            });

            return userDto;
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(int id)
        {
            var user = await _repo.GetUserByIdAsync(id);
            return user == null ? null : MapToDetailDto(user);
        }

        // Search + Filter + Pagination
        public async Task<PagedResult<UserSummaryDto>> GetUsersAsync(string? search, string? role, int pageNumber, int pageSize)
        {
            Enums? parsedRole = null;
            if (!string.IsNullOrEmpty(role) && Enum.TryParse<Enums>(role, true, out var parsed))
            {
                parsedRole = parsed;
            }

            var result = await _repo.GetUsersAsync(search, parsedRole, pageNumber, pageSize);

            return new PagedResult<UserSummaryDto>
            {
                Items = result.Items.Select(u => new UserSummaryDto
                {
                    UserId = u.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role.ToString(),
                    Status = u.Status
                }),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        public async Task<UserDetailDto?> UpdateUserAsync(int id, UserUpdateDto dto)
        {
            var user = await _repo.UpdateUserAsync(id, dto);
            if (user == null) return null;

            await WebSocketHandler.BroadcastObjectAsync(new
            {
                type = "USER_UPDATED",
                message = $"User updated: {user.FirstName} {user.LastName}",
                name = user.FirstName + " " + user.LastName,
                email = user.Email,
                timestamp = DateTime.UtcNow.ToString("o")
            });

            return MapToDetailDto(user);
        }

        public async Task<bool> ActivateUserAsync(int id)
        {
            var success = await _repo.ActivateUserAsync(id);
            if (success)
            {
                await WebSocketHandler.BroadcastObjectAsync(new
                {
                    type = "USER_ACTIVATED",
                    message = $"User ID {id} activated.",
                    timestamp = DateTime.UtcNow.ToString("o")
                });
            }
            return success;
        }

        public async Task<bool> DeactivateUserAsync(int id)
        {
            var success = await _repo.DeactivateUserAsync(id);
            if (success)
            {
                await WebSocketHandler.BroadcastObjectAsync(new
                {
                    type = "USER_DEACTIVATED",
                    message = $"User ID {id} deactivated.",
                    timestamp = DateTime.UtcNow.ToString("o")
                });
            }
            return success;
        }

        private static UserDetailDto MapToDetailDto(User user)
        {
            return new UserDetailDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Status = user.Status,
                Role = user.Role.ToString()
            };
        }

        public async Task<int> GetTotalUsersCountAsync() => await _repo.GetTotalUsersCountAsync();
        public async Task<int> GetActiveUsersCountAsync() => await _repo.GetActiveUsersCountAsync();
        public async Task<int> GetActiveCustomersCountAsync() => await _repo.GetActiveCustomersCountAsync();
        public async Task<int> GetActiveEmployeesCountAsync() => await _repo.GetActiveEmployeesCountAsync();
    }
}
