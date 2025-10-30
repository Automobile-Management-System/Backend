using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Policy = "AdminOnly")] // Temporarily commented for testing
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminDashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        /// <summary>
        /// Get complete admin dashboard data including stats, charts, recent users, and alerts
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<AdminDashboardDto>> GetDashboardData()
        {
            try
            {
                var dashboardData = await _adminDashboardService.GetDashboardDataAsync();
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching dashboard data", error = ex.Message });
            }
        }

        /// <summary>
        /// Get dashboard statistics (total users, active bookings, revenue, growth rate)
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<AdminDashboardStatsDto>> GetDashboardStats()
        {
            try
            {
                var stats = await _adminDashboardService.GetDashboardStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching dashboard stats", error = ex.Message });
            }
        }

        /// <summary>
        /// Get dashboard charts data (weekly revenue and appointments)
        /// </summary>
        [HttpGet("charts")]
        public async Task<ActionResult<AdminDashboardChartsDto>> GetDashboardCharts()
        {
            try
            {
                var charts = await _adminDashboardService.GetDashboardChartsAsync();
                return Ok(charts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching dashboard charts", error = ex.Message });
            }
        }

        /// <summary>
        /// Get recent users list
        /// </summary>
        [HttpGet("recent-users")]
        public async Task<ActionResult<List<RecentUserDto>>> GetRecentUsers([FromQuery] int limit = 10)
        {
            try
            {
                if (limit <= 0 || limit > 50)
                {
                    return BadRequest(new { message = "Limit must be between 1 and 50" });
                }

                var recentUsers = await _adminDashboardService.GetRecentUsersAsync(limit);
                return Ok(recentUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching recent users", error = ex.Message });
            }
        }

        /// <summary>
        /// Get system alerts
        /// </summary>
        [HttpGet("alerts")]
        public async Task<ActionResult<List<SystemAlertDto>>> GetSystemAlerts()
        {
            try
            {
                var alerts = await _adminDashboardService.GetSystemAlertsAsync();
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching system alerts", error = ex.Message });
            }
        }

        /// <summary>
        /// Mark a system alert as read
        /// </summary>
        [HttpPut("alerts/{alertId}/mark-read")]
        public async Task<ActionResult> MarkAlertAsRead(int alertId)
        {
            try
            {
                if (alertId <= 0)
                {
                    return BadRequest(new { message = "Invalid alert ID" });
                }

                var result = await _adminDashboardService.MarkAlertAsReadAsync(alertId);
                
                if (result)
                {
                    return Ok(new { message = "Alert marked as read successfully" });
                }
                else
                {
                    return NotFound(new { message = "Alert not found" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while marking alert as read", error = ex.Message });
            }
        }
    }
}