using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceAnalyticsController : ControllerBase
    {
        private readonly IServiceAnalyticsService _service;

        public ServiceAnalyticsController(IServiceAnalyticsService service)
        {
            _service = service;
        }

        [HttpGet("services")]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            var services = await _service.GetServicesAsync();
            return Ok(services);
        }
    }
}
