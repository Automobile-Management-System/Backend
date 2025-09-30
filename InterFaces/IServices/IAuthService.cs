using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;

namespace automobile_backend.InterFaces.IServices
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserRegisterDto request);
        Task<string?> LoginAsync(UserLoginDto request);
    }
}
