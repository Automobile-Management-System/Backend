using automobile_backend.InterFaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeeTimeLogController : ControllerBase
    {
        private readonly IEmployeeTimeLogService _timeLogService;

        public EmployeeTimeLogController(IEmployeeTimeLogService timeLogService)
        {
            _timeLogService = timeLogService;
        }

        [HttpGet("my-logs")]
        public async Task<IActionResult> GetMyTimeLogs(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "User not authenticated" });

                int userId = int.Parse(userIdClaim);
                var response = await _timeLogService.GetEmployeeTimeLogsAsync(userId, pageNumber, pageSize, search, startDate, endDate);

                return Ok(new
                {
                    success = true,
                    pagination = new
                    {
                        response.TotalCount,
                        response.PageNumber,
                        response.PageSize,
                        response.TotalPages,
                        response.HasNextPage,
                        response.HasPreviousPage
                    },
                    data = response.Data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
