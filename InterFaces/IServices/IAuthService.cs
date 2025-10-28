using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;

namespace automobile_backend.InterFaces.IServices
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserRegisterDto request);
        Task<(User? user, string? token)> LoginAsync(UserLoginDto request);

        Task<(User user, string jwtToken)> HandleGoogleLoginAsync();

        Task<User?> GetUserByEmailAsync(string email);
    }
}
