using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceProgressController : ControllerBase
    {
        private readonly IServiceProgressService _serviceProgressService;

        public ServiceProgressController(IServiceProgressService serviceProgressService)
        {
            _serviceProgressService = serviceProgressService;
        }

        /// <summary>
        /// Get all service progress for a specific employee
        /// </summary>
        /// <param name="employeeId">The ID of the employee</param>
        /// <returns>List of service progress items</returns>
        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<ServiceProgressDto>>> GetEmployeeServiceProgress(int employeeId)
        {
            try
            {
                var serviceProgress = await _serviceProgressService.GetEmployeeServiceProgressAsync(employeeId);
                return Ok(serviceProgress);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving service progress", error = ex.Message });
            }
        }

        /// <summary>
        /// Get service progress for a specific appointment
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment</param>
        /// <returns>Service progress details</returns>
        [HttpGet("appointment/{appointmentId}")]
        public async Task<ActionResult<ServiceProgressDto>> GetServiceProgressById(int appointmentId)
        {
            try
            {
                var serviceProgress = await _serviceProgressService.GetServiceProgressByIdAsync(appointmentId);
                if (serviceProgress == null)
                {
                    return NotFound(new { message = "Service progress not found" });
                }
                return Ok(serviceProgress);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving service progress", error = ex.Message });
            }
        }

        /// <summary>
        /// Start timer for a service appointment
        /// </summary>
        /// <param name="timerAction">Timer action details</param>
        /// <returns>Timer response</returns>
        [HttpPost("timer/start")]
        public async Task<ActionResult<TimerResponseDto>> StartTimer([FromBody] TimerActionDto timerAction)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _serviceProgressService.StartTimerAsync(timerAction);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error starting timer", error = ex.Message });
            }
        }

        /// <summary>
        /// Pause timer for a service appointment
        /// </summary>
        /// <param name="timerAction">Timer action details</param>
        /// <returns>Timer response</returns>
        [HttpPost("timer/pause")]
        public async Task<ActionResult<TimerResponseDto>> PauseTimer([FromBody] TimerActionDto timerAction)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _serviceProgressService.PauseTimerAsync(timerAction);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error pausing timer", error = ex.Message });
            }
        }

        /// <summary>
        /// Stop timer and complete service for an appointment
        /// </summary>
        /// <param name="timerAction">Timer action details</param>
        /// <returns>Timer response</returns>
        [HttpPost("timer/stop")]
        public async Task<ActionResult<TimerResponseDto>> StopTimer([FromBody] TimerActionDto timerAction)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _serviceProgressService.StopTimerAsync(timerAction);
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error stopping timer", error = ex.Message });
            }
        }

        /// <summary>
        /// Update service status
        /// </summary>
        /// <param name="updateStatus">Status update details</param>
        /// <returns>Success response</returns>
        [HttpPut("status")]
        public async Task<ActionResult> UpdateServiceStatus([FromBody] UpdateStatusDto updateStatus)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _serviceProgressService.UpdateServiceStatusAsync(updateStatus);
                if (!result)
                {
                    return BadRequest(new { message = "Failed to update service status" });
                }

                return Ok(new { message = "Service status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating service status", error = ex.Message });
            }
        }

        /// <summary>
        /// Get active timer for a specific appointment and user
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment</param>
        /// <param name="userId">The ID of the user</param>
        /// <returns>Active timer details</returns>
        [HttpGet("timer/active/{appointmentId}/{userId}")]
        public async Task<ActionResult<TimeLogDto>> GetActiveTimer(int appointmentId, int userId)
        {
            try
            {
                var activeTimer = await _serviceProgressService.GetActiveTimerAsync(appointmentId, userId);
                if (activeTimer == null)
                {
                    return NotFound(new { message = "No active timer found" });
                }

                return Ok(activeTimer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving active timer", error = ex.Message });
            }
        }

        /// <summary>
        /// Get total logged time for an appointment
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment</param>
        /// <returns>Total logged time in hours</returns>
        [HttpGet("time-logged/{appointmentId}")]
        public async Task<ActionResult<decimal>> GetTotalLoggedTime(int appointmentId)
        {
            try
            {
                var totalTime = await _serviceProgressService.GetTotalLoggedTimeAsync(appointmentId);
                return Ok(new { appointmentId, totalTimeLogged = totalTime });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving total logged time", error = ex.Message });
            }
        }
    }
}