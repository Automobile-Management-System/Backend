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
            return Ok(requests); // DTO has CreatedDateString & CreatedTimeString ready for frontend
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerModificationRequestDto requestDto) // ✅ Correct DTO
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ✅ Just pass DTO to service
            await _requestService.AddModificationRequestAsync(requestDto); // ✅ Now it works

            return Ok(new { message = "Modification request created successfully!" });
        }
    }
}