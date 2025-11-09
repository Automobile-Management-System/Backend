using automobile_backend.InterFaces.IServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

// DTO for review request - Simplified
public class ReviewRequestDto
{
    public string Action { get; set; } = string.Empty; // "approve" or "reject"
    public decimal? EstimatedCost { get; set; }
    public int? AssigneeId { get; set; } // Employee to assign for pending requests
    public object RespondedBy { get; internal set; }
}


namespace automobile_backend.Controllers
{
    [ApiController]
    [Route("api/admin/modification-requests")]
    public class AdminModificationReqController : ControllerBase
    {
        private readonly IModificationRequestService _service;
private readonly IEmployeeServiceWorkService _employeeService;
        public AdminModificationReqController(
            IModificationRequestService service,
            IEmployeeServiceWorkService employeeService) 
        {
            _service = service;
            _employeeService = employeeService; 
        }

       [HttpGet]
public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
{
    var (data, totalCount) = await _service.GetAllModificationRequestsAsync(pageNumber, pageSize);
    return Ok(new { data, totalCount });
}


        [HttpPut("{id}/review")]
        public async Task<IActionResult> ReviewRequest(int id, [FromBody] ReviewRequestDto reviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedRequest = await _service.ReviewModificationRequestAsync(id, reviewDto);

                if (updatedRequest == null)
                {
                    return NotFound(new { message = "Modification request not found" });
                }

                return Ok(updatedRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

              //  New endpoint: Get assigned appointments count for employees
        [HttpGet("assigned-appointments")]
        public async Task<IActionResult> GetEmployeeAssignedAppointmentCounts()
        {
            try
            {
                var result = await _employeeService.GetEmployeeAssignedAppointmentCountsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving assigned appointment counts", error = ex.Message });
            }
        }

        [HttpGet("all-employees-daily-count")]
public async Task<IActionResult> GetAllEmployeesWithDailyCount([FromQuery] DateTime date)
{
    try
    {
        var employees = await _employeeService.GetAllEmployeesWithDailyAssignmentCountAsync(date);
        return Ok(employees);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Error retrieving employees with daily counts", error = ex.Message });
    }
}
    }
}

