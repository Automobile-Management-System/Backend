using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;

namespace automobile_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync();
            
            // Map to DTOs to avoid circular references
            var appointmentDtos = appointments.Select(a => new AppointmentResponseDto
            {
                AppointmentId = a.AppointmentId,
                DateTime = a.DateTime,
                Status = a.Status,
                UserId = a.UserId,
                UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Unknown",
                Services = a.AppointmentServices?.Select(aps => new ServiceDto
                {
                    ServiceId = aps.Service.ServiceId,
                    ServiceName = aps.Service.ServiceName,
                    BasePrice = aps.Service.BasePrice
                }).ToList() ?? new List<ServiceDto>()
            }).ToList();
            
            return Ok(appointmentDtos);
        }

        [HttpGet("my-appointments")]
        [Authorize]
        public async Task<IActionResult> GetMyAppointments()
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var appointments = (await _appointmentService.GetAllAppointmentsAsync())
                .Where(a => a.UserId == userId)
                .ToList();
            
            // Map to DTOs
            var appointmentDtos = appointments.Select(a => new AppointmentResponseDto
            {
                AppointmentId = a.AppointmentId,
                DateTime = a.DateTime,
                Status = a.Status,
                UserId = a.UserId,
                UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Unknown",
                Services = a.AppointmentServices?.Select(aps => new ServiceDto
                {
                    ServiceId = aps.Service.ServiceId,
                    ServiceName = aps.Service.ServiceName,
                    BasePrice = aps.Service.BasePrice
                }).ToList() ?? new List<ServiceDto>()
            }).ToList();
            
            return Ok(appointmentDtos);
        }

        [HttpPost("create")]
        [Authorize] // optional: enable if using authentication
        public async Task<IActionResult> Create([FromBody] CreateServiceAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // In a real app, you'd get user ID from the JWT claims
            // e.g. int userId = int.Parse(User.FindFirst("userId")!.Value);
            // For now, assume test user id = 1
            int userId = 1;

            try
            {
                var appointment = await _appointmentService.CreateAppointmentAsync(userId, dto);
                
                // Return DTO instead of entity
                var appointmentDto = new AppointmentResponseDto
                {
                    AppointmentId = appointment.AppointmentId,
                    DateTime = appointment.DateTime,
                    Status = appointment.Status,
                    UserId = appointment.UserId,
                    Services = appointment.AppointmentServices?.Select(aps => new ServiceDto
                    {
                        ServiceId = aps.ServiceId,
                        ServiceName = aps.Service?.ServiceName ?? "Unknown",
                        BasePrice = aps.Service?.BasePrice ?? 0
                    }).ToList() ?? new List<ServiceDto>()
                };
                
                return Ok(appointmentDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
