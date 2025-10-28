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

        // Utility: Get logged-in user ID
        private int GetLoggedInUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User not logged in.");

            return int.Parse(userIdClaim);
        }

        [HttpGet("upcoming-count")]
        public async Task<IActionResult> GetUpcomingCount()
        {
            int userId = GetLoggedInUserId();
            var count = await _service.GetUpcomingAppointmentsCountAsync(userId);
            return Ok(new { upcomingCount = count });
        }

        [HttpGet("inprogress-count")]
        public async Task<IActionResult> GetInProgressCount()
        {
            int userId = GetLoggedInUserId();
            var count = await _service.GetInProgressAppointmentsCountAsync(userId);
            return Ok(new { inProgressCount = count });
        }

        [HttpGet("completed-count")]
        public async Task<IActionResult> GetCompletedCount()
        {
            int userId = GetLoggedInUserId();
            var count = await _service.GetCompletedAppointmentsCountAsync(userId);
            return Ok(new { completedCount = count });
        }

        [HttpGet("pending-payments-total")]
        public async Task<IActionResult> GetPendingPaymentsTotal()
        {
            int userId = GetLoggedInUserId();
            var total = await _service.GetPendingPaymentsTotalAsync(userId);
            return Ok(new { pendingPaymentsTotal = total });
        }

        [HttpGet("latest-services")]
        public async Task<IActionResult> GetLatestServices()
        {
            int userId = GetLoggedInUserId();
            var services = await _service.GetLatestServicesAsync(userId);
            return Ok(services);
        }

        [HttpGet("latest-modifications")]
        public async Task<IActionResult> GetLatestModifications()
        {
            int userId = GetLoggedInUserId();
            var modifications = await _service.GetLatestModificationsAsync(userId);
            return Ok(modifications);
        }
    }
}

