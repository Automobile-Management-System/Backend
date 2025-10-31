using automobile_backend.Models.DTOs;
using automobile_backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Only logged-in users
    public class CustomerVehicleController : ControllerBase
    {
        private readonly ICustomerVehicleService _vehicleService;

        public CustomerVehicleController(ICustomerVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet("my-vehicles")]
        public async Task<IActionResult> GetMyVehicles()
        {
            var userId = GetUserId();
            var vehicles = await _vehicleService.GetUserVehiclesAsync(userId);
            return Ok(vehicles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            var userId = GetUserId();
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id, userId);
            if (vehicle == null) return NotFound("Vehicle not found.");
            return Ok(vehicle);
        }

        [HttpPost]
        public async Task<IActionResult> AddVehicle([FromBody] CustomerVehicleDto dto)
        {
            var userId = GetUserId();
            await _vehicleService.AddVehicleAsync(userId, dto);
            return Ok("Vehicle added successfully.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] UpdateVehicleDto dto)
        {
            var userId = GetUserId();
            var success = await _vehicleService.UpdateVehicleAsync(id, userId, dto);
            if (!success) return NotFound("Vehicle not found or unauthorized.");
            return Ok("Vehicle updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var userId = GetUserId();
            var success = await _vehicleService.DeleteVehicleAsync(id, userId);
            if (!success) return NotFound("Vehicle not found or unauthorized.");
            return Ok("Vehicle deleted successfully.");
        }
    }
}
