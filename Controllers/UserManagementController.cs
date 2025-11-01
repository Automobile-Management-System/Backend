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

        [HttpPost("add-employee")]
        public async Task<IActionResult> AddEmployee([FromBody] UserRegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _service.AddEmployeeAsync(dto);
            return Ok(result);
        }

        //Get all users with search, role filter, and pagination
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] string? search,
            [FromQuery] string? role,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageSize > 10) pageSize = 10; // Max limit
            var users = await _service.GetUsersAsync(search, role, pageNumber, pageSize);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _service.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            var updated = await _service.UpdateUserAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpPut("activate/{id}")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var success = await _service.ActivateUserAsync(id);
            return success ? Ok("User activated") : NotFound();
        }

        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var success = await _service.DeactivateUserAsync(id);
            return success ? Ok("User deactivated") : NotFound();
        }

        // total count
        [HttpGet("count/total")]
        public async Task<IActionResult> GetTotalUsersCount()
        {
            var count = await _service.GetTotalUsersCountAsync();
            return Ok(count);
        }

        // active count
        [HttpGet("count/active")]
        public async Task<IActionResult> GetActiveUsersCount()
        {
            var count = await _service.GetActiveUsersCountAsync();
            return Ok(count);
        }

        // active customer count
        [HttpGet("count/active-customers")]
        public async Task<IActionResult> GetActiveCustomersCount()
        {
            var count = await _service.GetActiveCustomersCountAsync();
            return Ok(count);
        }

        // active employee count
        [HttpGet("count/active-employees")]
        public async Task<IActionResult> GetActiveEmployeesCount()
        {
            var count = await _service.GetActiveEmployeesCountAsync();
            return Ok(count);
        }

    }
}
