using automobile_backend.InterFaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class EmployeeDashboardController : ControllerBase
    {
        private readonly IEmployeeDashboardService _employeeDashboardService;

        public EmployeeDashboardController(IEmployeeDashboardService employeeDashboardService)
        {
            _employeeDashboardService = employeeDashboardService;
        }

        [HttpGet("appointments/today/upcoming-count")]
        public async Task<IActionResult> GetTodayUpcomingAppointmentsForLoggedEmployee()
        {
           
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            int employeeId = int.Parse(userIdClaim);

            var count = await _employeeDashboardService.GetTodayUpcomingAppointmentCountAsync(employeeId);

            return Ok(new
            {
                EmployeeId = employeeId,
                Date = DateTime.Today.ToString("yyyy-MM-dd"),
                UpcomingAppointmentCount = count
            });
        }

        [HttpGet("appointments/inprogress-count")]
        public async Task<IActionResult> GetInProgressAppointmentsForLoggedEmployee()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            int employeeId = int.Parse(userIdClaim);

            var count = await _employeeDashboardService.GetInProgressAppointmentCountAsync(employeeId);

            return Ok(new
            {
                EmployeeId = employeeId,
                Status = "InProgress",
                InProgressAppointmentCount = count
            });
        }

        [HttpGet("appointments/completed/services-count")]
        public async Task<IActionResult> GetCompletedServiceCount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            int employeeId = int.Parse(userIdClaim);
            var count = await _employeeDashboardService.GetCompletedServiceCountAsync(employeeId);

            return Ok(new
            {
                EmployeeId = employeeId,
                Status = "Completed",
                CompletedServiceCount = count
            });
        }


        [HttpGet("appointments/completed/modifications-count")]
        public async Task<IActionResult> GetCompletedModificationCount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            int employeeId = int.Parse(userIdClaim);
            var count = await _employeeDashboardService.GetCompletedModificationCountAsync(employeeId);

            return Ok(new
            {
                EmployeeId = employeeId,
                Status = "Completed",
                CompletedModificationCount = count
            });
        }



        [HttpGet("appointments/today/recent-services")]
        public async Task<IActionResult> GetTodayRecentServicesForLoggedEmployee()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            int employeeId = int.Parse(userIdClaim);

            var recentServices = await _employeeDashboardService.GetTodayRecentServicesAsync(employeeId);

            return Ok(new
            {
                EmployeeId = employeeId,
                Date = DateTime.Today.ToString("yyyy-MM-dd"),
                RecentServices = recentServices
            });
        }

        [HttpGet("appointments/today/recent-modifications")]
        public async Task<IActionResult> GetTodayRecentModificationsForLoggedEmployee()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            int employeeId = int.Parse(userIdClaim);

            var recentModifications = await _employeeDashboardService.GetTodayRecentModificationsAsync(employeeId);

            return Ok(new
            {
                EmployeeId = employeeId,
                Date = DateTime.Today.ToString("yyyy-MM-dd"),
                RecentModifications = recentModifications
            });
        }

    }
}
