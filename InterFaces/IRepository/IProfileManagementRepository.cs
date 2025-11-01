using automobile_backend.Models.Entities;
using automobile_backend.Models.DTOs;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IProfileManagementRepository
    {
        // Optimized method for fetching profile data directly as DTO
        Task<ProfileManagementDto?> GetProfileDtoByIdAsync(int id);
        
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        
        // Optimized method to check email existence
        Task<bool> EmailExistsAsync(string email, int excludeUserId = 0);
        
        // Lightweight method for updates
        Task<User?> GetUserForUpdateAsync(int id);
        
        Task UpdateAsync(User user);
        Task SaveChangesAsync();
        Task<User?> GetByIdWithRelatedDataAsync(int id); // Add this if needed elsewhere
    }
}