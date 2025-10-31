using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

// DTO for employee assignment
public class AssignEmployeeDto
{
    public int EmployeeId { get; set; }
}

namespace automobile_backend.Controllers
{
    [ApiController]
    [Route("api/admin/service-appointments")]
    public class AdminServiceAppointmentController : ControllerBase
    {
        private readonly IServiceAppointmentService _service;

        public AdminServiceAppointmentController(IServiceAppointmentService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all service appointments with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (data, totalCount) = await _service.GetAllServiceAppointmentsAsync(pageNumber, pageSize);
                return Ok(new { data, totalCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving service appointments", error = ex.Message });
            }
        }

        /// <summary>
        /// Get detailed information about a specific service appointment
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var appointment = await _service.GetServiceAppointmentByIdAsync(id);

                if (appointment == null)
                {
                    return NotFound(new { message = "Service appointment not found" });
                }

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving service appointment", error = ex.Message });
            }
        }

        /// <summary>
        /// Get available employees for a specific date and time slot
        /// </summary>
        [HttpGet("available-employees")]
        public async Task<IActionResult> GetAvailableEmployees(
            [FromQuery] DateTime date, 
            [FromQuery] SlotsTime slotTime)
        {
            try
            {
                var availableEmployees = await _service.GetAvailableEmployeesForAssignmentAsync(date, slotTime);
                return Ok(availableEmployees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving available employees", error = ex.Message });
            }
        }

        /// <summary>
        /// Assign an employee to a service appointment
        /// </summary>
        [HttpPost("{id}/assign-employee")]
        public async Task<IActionResult> AssignEmployee(int id, [FromBody] AssignEmployeeDto assignDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.AssignEmployeeToAppointmentAsync(id, assignDto.EmployeeId);

                if (result == null)
                {
                    return NotFound(new { message = "Service appointment not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("employee-assignments")]
public async Task<IActionResult> GetEmployeeAssignments([FromQuery] int employeeId, [FromQuery] DateTime date)
{
    try
    {
        var assignments = await _service.GetEmployeeAssignmentsAsync(employeeId, date);
        return Ok(assignments);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Error fetching employee assignments", error = ex.Message });
    }
}

    }
}