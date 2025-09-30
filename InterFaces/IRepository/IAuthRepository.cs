using automobile_backend.Models.Entities;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
    }
}
