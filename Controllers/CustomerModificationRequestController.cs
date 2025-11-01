using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace automobile_backend.Controllers
{
    [ApiController]
    [Route("api/customer-modification-requests")]
    [Authorize(Roles = "Customer")]
    public class CustomerModificationRequestController : ControllerBase
    {
        private readonly ICustomerModificationRequestService _service;

        public CustomerModificationRequestController(ICustomerModificationRequestService service)
        {
            _service = service;
        }

        // GET: api/customer-modification-requests
        [HttpGet]
        public async Task<IActionResult> GetForLoggedInUser()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized();

                var requests = await _service.GetByUserIdAsync(userId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching modification requests", error = ex.Message });
            }
        }

        // POST: api/customer-modification-requests
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerModificationRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized();

                dto.UserId = userId; // Assign logged-in user automatically

                await _service.AddModificationRequestAsync(dto);

                return Ok(new { message = "Modification request created successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating modification request", error = ex.Message });
            }
        }
    }
}
