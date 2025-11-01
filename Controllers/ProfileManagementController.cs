using System.Security.Claims;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileManagementController : ControllerBase
    {
        private readonly IProfileManagementService _service;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ProfileManagementController> _logger;

        public ProfileManagementController(
            IProfileManagementService service, 
            IMemoryCache cache,
            ILogger<ProfileManagementController> logger)
        {
            _service = service;
            _cache = cache;
            _logger = logger;
        }

        // GET api/profilemanagement
        [HttpGet]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var userId = GetUserIdFromClaims();
            if (userId == null) return Unauthorized();

            // Create cache key for this user's profile
            var cacheKey = $"user_profile_{userId}";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out ProfileManagementDto cachedProfile))
            {
                _logger.LogInformation("Returning cached profile for user {UserId}", userId);
                return Ok(cachedProfile);
            }

            var profile = await _service.GetCurrentUserProfileAsync(userId.Value);
            if (profile == null) return NotFound();

            // Cache the profile for 5 minutes
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };
            
            _cache.Set(cacheKey, profile, cacheOptions);
            _logger.LogInformation("Cached profile for user {UserId}", userId);

            return Ok(profile);
        }

        // PUT api/profilemanagement
        [HttpPut]
        public async Task<IActionResult> UpdateCurrentUserProfile([FromBody] ProfileUpdateDto updateDto)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null) return Unauthorized();

            var (success, error) = await _service.UpdateCurrentUserProfileAsync(userId.Value, updateDto);
            if (!success) return BadRequest(new { success = false, error });

            // Remove cached profile after update
            var cacheKey = $"user_profile_{userId}";
            _cache.Remove(cacheKey);
            _logger.LogInformation("Removed cached profile for user {UserId} after update", userId);

            return NoContent(); // or return Ok(...) if you want the updated profile
        }

        private int? GetUserIdFromClaims()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idClaim, out int userId)) return userId;
            return null;
        }
    }
}
