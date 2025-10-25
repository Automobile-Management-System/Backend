using automobile_backend.InterFaces.IServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

// DTO for review request
public class ReviewRequestDto
{
    public string Action { get; set; } = string.Empty;
    public string AdminResponse { get; set; } = string.Empty;
    public decimal? EstimatedCost { get; set; }
    public int RespondedBy { get; set; }
}

namespace automobile_backend.Controllers
{
    [ApiController]
[Route("api/admin/modification-requests")]
public class AdminModificationReqController : ControllerBase
    {
        private readonly IModificationRequestService _service;

        public AdminModificationReqController(IModificationRequestService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _service.GetAllModificationRequestsAsync();
            return Ok(requests);
        }

        [HttpPut("{id}/review")]
        public async Task<IActionResult> ReviewRequest(int id, [FromBody] ReviewRequestDto reviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedRequest = await _service.ReviewModificationRequestAsync(id, reviewDto);
                
                if (updatedRequest == null)
                {
                    return NotFound(new { message = "Modification request not found" });
                }

                return Ok(updatedRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}