using automobile_backend.InterFaces.IServices;
using Microsoft.AspNetCore.Mvc;

namespace automobile_backend.Controllers
{
    [ApiController]
    [Route("api/customer-vehicles")]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetVehiclesByUserId(int userId)
        {
            var vehicles = await _vehicleService.GetVehiclesByUserIdAsync(userId);
            return Ok(vehicles);
        }
    }
}
