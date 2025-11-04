using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.Entities;
using automobile_backend.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace automobile_backend.Repository
{
    public class ProfileManagementRepository : IProfileManagementRepository
    {
        private readonly ApplicationDbContext _db;

        public ProfileManagementRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        // Optimized method that only selects the specific fields needed for profile
        public async Task<ProfileManagementDto?> GetProfileDtoByIdAsync(int id)
        {
            return await _db.Users
                .AsNoTracking()
                .Where(u => u.UserId == id)
                .Select(u => new ProfileManagementDto
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    ProfilePicture = u.ProfilePicture,
                    Status = u.Status
                })
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            // Only load user data without related entities for profile operations
            return await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        // Optimized method to check if email exists (faster than fetching full user)
        public async Task<bool> EmailExistsAsync(string email, int excludeUserId = 0)
        {
            return await _db.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == email && u.UserId != excludeUserId);
        }

        // Lightweight method to get user for updates
        public async Task<User?> GetUserForUpdateAsync(int id)
        {
            return await _db.Users
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        // Add this new method if you need to load related data elsewhere
        public async Task<User?> GetByIdWithRelatedDataAsync(int id)
        {
            return await _db.Users
                .Include(u => u.CustomerVehicles)
                .Include(u => u.Appointments)
                .Include(u => u.TimeLogs)
                .Include(u => u.Reports)
                .Include(u => u.EmployeeAppointments)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }
    }
}
