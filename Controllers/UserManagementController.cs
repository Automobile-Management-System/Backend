using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _service;

        public UserManagementController(IUserManagementService service)
        {
            _service = service;
        }

        // Add employee user
        [HttpPost("add-employee")]
        public async Task<IActionResult> AddEmployee([FromBody] UserRegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.AddEmployeeAsync(dto);
            return Ok(result);
        }

        // Get all users (summary)
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _service.GetAllUsersAsync();
            return Ok(users);
        }

        // Get single user details
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _service.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // Update user
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            var updated = await _service.UpdateUserAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // Activate user
        [HttpPut("activate/{id}")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var success = await _service.ActivateUserAsync(id);
            return success ? Ok("User activated") : NotFound();
        }

        // Deactivate user
        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var success = await _service.DeactivateUserAsync(id);
            return success ? Ok("User deactivated") : NotFound();
        }
    }
}
