using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminDashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        /// <summary>
        /// Get dashboard overview cards (Total Revenue, Total Users, Total Customers, Total Appointments)
        /// </summary>
        [HttpGet("overview")]
        public async Task<ActionResult<AdminDashboardOverviewDto>> GetDashboardOverview()
        {
            try
            {
                var overview = await _adminDashboardService.GetDashboardOverviewAsync();
                return Ok(overview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching dashboard overview", error = ex.Message });
            }
        }

        /// <summary>
        /// Get weekly revenue data for graph (Monday to Sunday)
        /// </summary>
        [HttpGet("weekly-revenue")]
        public async Task<ActionResult<WeeklyRevenueDto>> GetWeeklyRevenue()
        {
            try
            {
                var weeklyRevenue = await _adminDashboardService.GetWeeklyRevenueAsync();
                return Ok(weeklyRevenue);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching weekly revenue", error = ex.Message });
            }
        }

        /// <summary>
        /// Get weekly appointments data for graph (Monday to Sunday)
        /// </summary>
        [HttpGet("weekly-appointments")]
        public async Task<ActionResult<WeeklyAppointmentsDto>> GetWeeklyAppointments()
        {
            try
            {
                var weeklyAppointments = await _adminDashboardService.GetWeeklyAppointmentsAsync();
                return Ok(weeklyAppointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching weekly appointments", error = ex.Message });
            }
        }

        /// <summary>
        /// Get recent users list
        /// </summary>
        [HttpGet("recent-users")]
        public async Task<ActionResult<IEnumerable<RecentUserDto>>> GetRecentUsers([FromQuery] int count = 10)
        {
            try
            {
                if (count <= 0 || count > 50)
                {
                    return BadRequest(new { message = "Count must be between 1 and 50" });
                }

                var recentUsers = await _adminDashboardService.GetRecentUsersAsync(count);
                return Ok(recentUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching recent users", error = ex.Message });
            }
        }
    }
}