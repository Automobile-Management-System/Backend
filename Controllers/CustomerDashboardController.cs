using automobile_backend.Interfaces.IServices;
using automobile_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Ensure user is logged in
    public class CustomerDashboardController : ControllerBase
    {
        private readonly ICustomerDashboardService _service;

        public CustomerDashboardController(ICustomerDashboardService service)
        {
            _service = service;
        }

        // POST: api/CustomerDashboard/vehicles
        [HttpPost("vehicles")]
        public async Task<IActionResult> AddVehicle([FromBody] CustomerVehicleDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("User not logged in.");

            int userId = int.Parse(userIdClaim);

            await _service.AddVehicleAsync(userId, dto);
            return Ok(new { message = "Vehicle added successfully." });
        }

        // GET: api/CustomerDashboard/vehicles
        [HttpGet("vehicles")]
        public async Task<IActionResult> GetUserVehicles()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("User not logged in.");

            int userId = int.Parse(userIdClaim);

            var vehicles = await _service.GetVehiclesByUserIdAsync(userId);
            return Ok(vehicles);
        }
    }
}
