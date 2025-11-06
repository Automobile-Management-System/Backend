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

        [HttpGet("history")]
        public async Task<IActionResult> GetEmployeeTimeLogHistory(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = "")
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await _timeLogService.GetEmployeeLogsAsync(
                userId,
                pageNumber,
                pageSize,
                search
            );

            return Ok(result);
        }
    }
}
