using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;

namespace automobile_backend.InterFaces.IServices
{
    public interface IProfileManagementService
    {
        Task<ProfileManagementDto?> GetCurrentUserProfileAsync(int userId);
        Task<(bool Success, string? ErrorMessage)> UpdateCurrentUserProfileAsync(int userId, ProfileUpdateDto updateDto);
        Task<User?> GetUserEntityByIdAsync(int userId);
    }
}