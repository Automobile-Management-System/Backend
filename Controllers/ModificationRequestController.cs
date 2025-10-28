using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace automobile_backend.Controllers
{
    [ApiController]
    [Route("api/modification-requests")]
    public class ModificationRequestController : ControllerBase
    {
        private readonly IModificationRequestService _requestService;

        public ModificationRequestController(IModificationRequestService requestService)
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
        public async Task<IActionResult> Create([FromBody] ModificationRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ✅ Just pass DTO to service
            await _requestService.AddModificationRequestAsync(requestDto);

            return Ok(new { message = "Modification request created successfully!" });
        }
    }
}
