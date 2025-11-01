using automobile_backend.Models.Entities;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IProfileManagementRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task UpdateAsync(User user);
        Task SaveChangesAsync();
    }
}