using automobile_backend.InterFaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Only logged-in employees
    public class EmployeeTimeLogController : ControllerBase
    {
        private readonly IEmployeeTimeLogService _timeLogService;

        public EmployeeTimeLogController(IEmployeeTimeLogService timeLogService)
        {
            _timeLogService = timeLogService;
        }

        [HttpGet("my-logs")]
        public async Task<IActionResult> GetMyTimeLogs()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                int userId = int.Parse(userIdClaim);
                var logs = await _timeLogService.GetEmployeeTimeLogsAsync(userId);

                return Ok(new
                {
                    success = true,
                    count = logs.Count,
                    data = logs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
