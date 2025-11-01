using automobile_backend.Models.DTOs;
using automobile_backend.Services;
using Microsoft.AspNetCore.Mvc;
using automobile_backend.Models.Entities;


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
        public async Task<ActionResult<IEnumerable<AdminPaymentDetailDto>>> GetAllPayments(
                    [FromQuery] int pageNumber = 1,
                    [FromQuery] string? search = null,
                    [FromQuery] PaymentStatus? status = null,
                    [FromQuery] PaymentMethod? paymentMethod = null)
        {
            try
            {
                // --- UPDATED SERVICE CALL ---
                // Pass all parameters to the service
                var (payments, totalCount) = await _paymentService.GetAllPaymentsAsync(
                    pageNumber,
                    DefaultPageSize,
                    search,
                    status,
                    paymentMethod
                );

                Response.Headers["X-Total-Count"] = totalCount.ToString();
                Response.Headers["Access-Control-Expose-Headers"] = "X-Total-Count";

                return Ok(payments);
            }
            catch (Exception ex)
            {
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

        // --- NEW ENDPOINT: Get Pending Count ---
        [HttpGet("revenue")]
        [ProducesResponseType(typeof(object), 200)] // Returns { "totalRevenue": 123.45 }
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTotalRevenue()
        {
            try
            {
                var revenue = await _paymentService.GetTotalRevenueAsync();
                return Ok(new { TotalRevenue = revenue });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // --- NEW ENDPOINT: Get Pending Count ---
        [HttpGet("count/pending")]
        [ProducesResponseType(typeof(object), 200)] // Returns { "count": 5 }
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetPendingCount()
        {
            try
            {
                var count = await _paymentService.GetPendingCountAsync();
                return Ok(new { Count = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // --- NEW ENDPOINT: Get Completed Count ---
        [HttpGet("count/completed")]
        [ProducesResponseType(typeof(object), 200)] // Returns { "count": 10 }
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCompletedCount()
        {
            try
            {
                var count = await _paymentService.GetCompletedCountAsync();
                return Ok(new { Count = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // --- NEW ENDPOINT: Get Failed Count ---
        [HttpGet("count/failed")]
        [ProducesResponseType(typeof(object), 200)] // Returns { "count": 2 }
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetFailedCount()
        {
            try
            {
                var count = await _paymentService.GetFailedCountAsync();
                return Ok(new { Count = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("report")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetPaymentsReport(
            [FromQuery] string? search = null,
            [FromQuery] PaymentStatus? status = null,
            [FromQuery] PaymentMethod? paymentMethod = null)
        {
            try
            {
                // Call a new service method to generate the report
                var (pdfBytes, fileName) = await _paymentService.GeneratePaymentsReportAsync(search, status, paymentMethod);

                // Return the PDF as a file
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //
    }
}