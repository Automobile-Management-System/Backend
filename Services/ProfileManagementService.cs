using System.Security.Cryptography;
using System.Text;
using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;

namespace automobile_backend.Services
{
    public class ProfileManagementService : IProfileManagementService
    {
        private readonly IProfileManagementRepository _repo;

        public ProfileManagementService(IProfileManagementRepository repo)
        {
            _repo = repo;
        }

        public async Task<ProfileManagementDto?> GetCurrentUserProfileAsync(int userId)
        {
            // Use the optimized method that directly returns DTO without loading the full entity
            return await _repo.GetProfileDtoByIdAsync(userId);
        }

        public async Task<User?> GetUserEntityByIdAsync(int userId)
        {
            return await _repo.GetByIdAsync(userId);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateCurrentUserProfileAsync(int userId, ProfileUpdateDto updateDto)
        {
            // Use lightweight method for getting user for updates
            var user = await _repo.GetUserForUpdateAsync(userId);
            if (user == null) return (false, "User not found.");

            // If email change requested, use optimized email existence check
            if (!string.IsNullOrWhiteSpace(updateDto.Email) && !string.Equals(updateDto.Email, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailExists = await _repo.EmailExistsAsync(updateDto.Email, userId);
                if (emailExists)
                {
                    return (false, "Email already in use by another account.");
                }

                user.Email = updateDto.Email.Trim();
            }

            // Update non-null fields (do not allow changing Role)
            if (!string.IsNullOrWhiteSpace(updateDto.FirstName)) user.FirstName = updateDto.FirstName;
            if (!string.IsNullOrWhiteSpace(updateDto.LastName)) user.LastName = updateDto.LastName;
            if (updateDto.PhoneNumber != null) user.PhoneNumber = updateDto.PhoneNumber;
            if (updateDto.Address != null) user.Address = updateDto.Address;
            if (updateDto.ProfilePicture != null) user.ProfilePicture = updateDto.ProfilePicture;
            if (updateDto.Status != null) user.Status = updateDto.Status;

            // Password change
            if (!string.IsNullOrWhiteSpace(updateDto.NewPassword))
            {
                CreatePasswordHash(updateDto.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            await _repo.UpdateAsync(user);
            await _repo.SaveChangesAsync();

            return (true, null);
        }

        // --- Password helpers (same approach as AuthService) ---
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
