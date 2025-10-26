using automobile_backend.InterFaces.IRepositories;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;

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
            return MapToDetailDto(user);
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(int id)
        {
            var user = await _repo.GetUserByIdAsync(id);
            return user == null ? null : MapToDetailDto(user);
        }

        public async Task<IEnumerable<UserSummaryDto>> GetAllUsersAsync()
        {
            var users = await _repo.GetAllUsersAsync();
            return users.Select(u => new UserSummaryDto
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Status = u.Status
            });
        }

        public async Task<UserDetailDto?> UpdateUserAsync(int id, UserUpdateDto dto)
        {
            var user = await _repo.UpdateUserAsync(id, dto);
            return user == null ? null : MapToDetailDto(user);
        }

        public async Task<bool> ActivateUserAsync(int id) => await _repo.ActivateUserAsync(id);

        public async Task<bool> DeactivateUserAsync(int id) => await _repo.DeactivateUserAsync(id);

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
    }
}
