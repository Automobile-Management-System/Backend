using automobile_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace automobile_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ViewServiceController : ControllerBase
    {
        private readonly IViewServiceService _viewServiceService;

        public ViewServiceController(IViewServiceService viewServiceService)
        {
            _viewServiceService = viewServiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllViewServices(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? search = null)
        {
            var pagedServices = await _viewServiceService.GetAllViewServicesAsync(pageNumber, pageSize, search);
            return Ok(pagedServices);
        }
    }
}
