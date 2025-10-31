using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace automobile_backend.Controllers
{
    [ApiController]
    [Route("api/customer-modification-requests")]
    public class CustomerModificationRequestController : ControllerBase
    {
        private readonly ICustomerModificationRequestService _requestService;

        public CustomerModificationRequestController(ICustomerModificationRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _requestService.GetAllModificationRequestsAsync();
            return Ok(requests);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerModificationRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _requestService.AddModificationRequestAsync(requestDto);
            return Ok(new { message = "Modification request created successfully!" });
        }

        // ✅ New endpoint
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var requests = await _requestService.GetByUserIdAsync(userId);
            return Ok(requests);
        }
    }
}
