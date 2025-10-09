using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeManagementWorkController : ControllerBase
    {
        private readonly IEmployeeManagementWorkService _service;

        public EmployeeManagementWorkController(IEmployeeManagementWorkService service)
        {
            _service = service;
        }

        [HttpGet("appointments")]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
        {
            var appointments = await _service.GetAppointmentsAsync();
            return Ok(appointments);
        }


    }
}
