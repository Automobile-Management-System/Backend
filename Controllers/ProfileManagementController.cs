using System.Security.Claims;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileManagementController : ControllerBase
    {
        private readonly IProfileManagementService _service;

        public ProfileManagementController(IProfileManagementService service)
        {
            _service = service;
        }

        // GET api/profilemanagement
        [HttpGet]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var userId = GetUserIdFromClaims();
            if (userId == null) return Unauthorized();

            var profile = await _service.GetCurrentUserProfileAsync(userId.Value);
            if (profile == null) return NotFound();

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
