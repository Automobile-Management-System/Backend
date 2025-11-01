using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System;

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

            var appointmentDtos = appointments.Select(a => new AppointmentResponseDto
            {
                AppointmentId = a.AppointmentId,
                DateTime = a.DateTime,
                Status = a.Status,
                UserId = a.UserId,
                UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Unknown",
                VehicleId = a.VehicleId,
                RegistrationNumber = a.CustomerVehicle?.RegistrationNumber ?? string.Empty,
                Services = a.AppointmentServices?.Select(aps => new automobile_backend.Models.DTOs.ServiceDto
                {
                    ServiceName = aps.Service.ServiceName,
                    BasePrice = aps.Service.BasePrice
                }).ToList() ?? new List<automobile_backend.Models.DTOs.ServiceDto>()
            }).ToList();

            return Ok(appointmentDtos);
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetAllPaginated([FromQuery] PaginationParameters parameters)
        {
            var paginatedAppointments = await _appointmentService.GetAllAppointmentsPaginatedAsync(parameters);

            var appointmentDtos = paginatedAppointments.Data.Select(a => new AppointmentResponseDto
            {
                AppointmentId = a.AppointmentId,
                DateTime = a.DateTime,
                Status = a.Status,
                UserId = a.UserId,
                UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Unknown",
                VehicleId = a.VehicleId,
                RegistrationNumber = a.CustomerVehicle?.RegistrationNumber ?? string.Empty,
                Services = a.AppointmentServices?.Select(aps => new automobile_backend.Models.DTOs.ServiceDto
                {
                    ServiceName = aps.Service?.ServiceName ?? "Unknown",
                    BasePrice = aps.Service?.BasePrice ?? 0
                }).ToList() ?? new List<automobile_backend.Models.DTOs.ServiceDto>()
            }).ToList();

            var result = new PaginatedResult<AppointmentResponseDto>
            {
                Data = appointmentDtos,
                TotalItems = paginatedAppointments.TotalItems,
                TotalPages = paginatedAppointments.TotalPages,
                CurrentPage = paginatedAppointments.CurrentPage,
                PageSize = paginatedAppointments.PageSize
            };

            return Ok(result);
        }

        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailability([FromQuery] DateTime date)
        {
            // Expecting date as yyyy-MM-dd; if not provided, use today's date
            var day = date == default ? DateTime.UtcNow.Date : date.Date;
            var availability = await _appointmentService.GetSlotAvailabilityAsync(day);
            return Ok(availability);
        }

        [HttpGet("my-appointments")]
        [Authorize]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var appointments = (await _appointmentService.GetAllAppointmentsAsync())
                .Where(a => a.UserId == userId)
                .ToList();

            var appointmentDtos = appointments.Select(a => new AppointmentResponseDto
            {
                AppointmentId = a.AppointmentId,
                DateTime = a.DateTime,
                Status = a.Status,
                UserId = a.UserId,
                UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Unknown",
                VehicleId = a.VehicleId,
                RegistrationNumber = a.CustomerVehicle?.RegistrationNumber ?? string.Empty,
                Services = a.AppointmentServices?.Select(aps => new automobile_backend.Models.DTOs.ServiceDto
                {
                    ServiceName = aps.Service.ServiceName,
                    BasePrice = aps.Service.BasePrice
                }).ToList() ?? new List<automobile_backend.Models.DTOs.ServiceDto>()
            }).ToList();

            return Ok(appointmentDtos);
        }

        [HttpGet("my-appointments/paginated")]
        [Authorize]
        public async Task<IActionResult> GetMyAppointmentsPaginated([FromQuery] PaginationParameters parameters)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var paginatedAppointments = await _appointmentService.GetUserAppointmentsPaginatedAsync(userId, parameters);

            var appointmentDtos = paginatedAppointments.Data.Select(a => new AppointmentResponseDto
            {
                AppointmentId = a.AppointmentId,
                DateTime = a.DateTime,
                Status = a.Status,
                UserId = a.UserId,
                UserName = a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : "Unknown",
                VehicleId = a.VehicleId,
                RegistrationNumber = a.CustomerVehicle?.RegistrationNumber ?? string.Empty,
                Services = a.AppointmentServices?.Select(aps => new automobile_backend.Models.DTOs.ServiceDto
                {
                    ServiceName = aps.Service?.ServiceName ?? "Unknown",
                    BasePrice = aps.Service?.BasePrice ?? 0
                }).ToList() ?? new List<automobile_backend.Models.DTOs.ServiceDto>()
            }).ToList();

            var result = new PaginatedResult<AppointmentResponseDto>
            {
                Data = appointmentDtos,
                TotalItems = paginatedAppointments.TotalItems,
                TotalPages = paginatedAppointments.TotalPages,
                CurrentPage = paginatedAppointments.CurrentPage,
                PageSize = paginatedAppointments.PageSize
            };

            return Ok(result);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateServiceAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            try
            {
                var appointment = await _appointmentService.CreateAppointmentAsync(userId, dto);

                var appointmentDto = new AppointmentResponseDto
                {
                    AppointmentId = appointment.AppointmentId,
                    DateTime = appointment.StartDateTime, // Changed from appointment.DateTime
                    Status = appointment.Status,
                    UserId = appointment.UserId,
                    VehicleId = appointment.VehicleId,
                    RegistrationNumber = appointment.CustomerVehicle?.RegistrationNumber ?? string.Empty,
                    Services = appointment.AppointmentServices?.Select(aps => new automobile_backend.Models.DTOs.ServiceDto
                    {
                        ServiceName = aps.Service?.ServiceName ?? "Unknown",
                        BasePrice = aps.Service?.BasePrice ?? 0
                    }).ToList() ?? new List<automobile_backend.Models.DTOs.ServiceDto>()
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
