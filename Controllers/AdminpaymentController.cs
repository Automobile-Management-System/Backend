using automobile_backend.Models.DTOs;
using automobile_backend.Services;
using Microsoft.AspNetCore.Mvc;


namespace automobile_backend.Controllers
{
    [Route("api/admin/payments")]
    [ApiController]
    public class AdminpaymentController : ControllerBase
    {
        private readonly IAdminpaymentService _paymentService;

        // Define the page size constant
        private const int DefaultPageSize = 10;

        public AdminpaymentController(IAdminpaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AdminPaymentDetailDto>), 200)]
        [ProducesResponseType(500)]
        // MODIFIED: Accepts pageNumber from query string, defaults to 1
        public async Task<ActionResult<IEnumerable<AdminPaymentDetailDto>>> GetAllPayments([FromQuery] int pageNumber = 1)
        {
            try
            {
                // Call the service with pagination
                var (payments, totalCount) = await _paymentService.GetAllPaymentsAsync(pageNumber, DefaultPageSize);

                // Add the total count to the response header
                Response.Headers["X-Total-Count"] = totalCount.ToString();

                // IMPORTANT: Expose the header so the frontend can read it (CORS)
                Response.Headers["Access-Control-Expose-Headers"] = "X-Total-Count";

                return Ok(payments);
            }
            catch (Exception ex)
            {
                // Log the exception (using a proper logging framework)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPatch("{id:int}")]
        [ProducesResponseType(204)] // No Content (Success)
        [ProducesResponseType(400)] // Bad Request (e.g., invalid JSON or enum value)
        [ProducesResponseType(404)] // Not Found
        [ProducesResponseType(500)] // Internal Server Error
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusDto dto)
        {
            // Model binding automatically validates the [Required] and enum constraints
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var success = await _paymentService.UpdatePaymentStatusAsync(id, dto.Status);

                if (!success)
                {
                    // The service returned false, meaning the payment ID was not found
                    return NotFound($"Payment with ID {id} not found.");
                }

                // Return 204 No Content, the standard response for a successful update
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception (using a proper logging framework)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}