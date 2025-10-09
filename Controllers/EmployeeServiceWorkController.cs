using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeServiceWorkController : ControllerBase
    {
        private readonly IEmployeeServiceWorkService _employeeWorkService;

        public EmployeeServiceWorkController(IEmployeeServiceWorkService employeeWorkService)
        {
            _employeeWorkService = employeeWorkService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimeLog>>> GetEmployeeWork()
        {
            var workItems = await _employeeWorkService.GetEmployeeWorkAsync();
            return Ok(workItems);
        }
    }
}
