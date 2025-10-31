using automobile_backend.InterFaces.IRepositories;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace automobile_backend.Repositories
{
    public class UserManagementRepository : IUserManagementRepository
    {
        private readonly ApplicationDbContext _context;

        public UserManagementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> AddEmployeeAsync(UserRegisterDto dto)
        {
            using var hmac = new HMACSHA512();
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password)),
                PasswordSalt = hmac.Key,
                Role = Enums.Employee,
                Status = "Active"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        //Search + Filter + Pagination
        public async Task<PagedResult<User>> GetUsersAsync(string? search, Enums? role, int pageNumber, int pageSize)
        {
            var query = _context.Users.AsQueryable();

            // Filter by search
            if (!string.IsNullOrWhiteSpace(search))
            {
                string normalized = search.ToLower();
                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(normalized) ||
                    u.LastName.ToLower().Contains(normalized) ||
                    u.Email.ToLower().Contains(normalized));
            }

            // Filter by role
            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            // Count total
            int totalCount = await query.CountAsync();

            // Pagination
            var items = await query
                .OrderBy(u => u.UserId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<User>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<User?> UpdateUserAsync(int id, UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.FirstName = dto.FirstName ?? user.FirstName;
            user.LastName = dto.LastName ?? user.LastName;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
            user.Address = dto.Address ?? user.Address;
            user.Status = dto.Status ?? user.Status;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> ActivateUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.Status = "Active";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            user.Status = "Inactive";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            return await _context.Users.CountAsync(u => u.Status == "Active");
        }

        public async Task<int> GetActiveCustomersCountAsync()
        {
            return await _context.Users.CountAsync(u => u.Status == "Active" && u.Role == Enums.Customer);
        }

        public async Task<int> GetActiveEmployeesCountAsync()
        {
            return await _context.Users.CountAsync(u => u.Status == "Active" && u.Role == Enums.Employee);
        }

    }
}
