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

        public AdminpaymentController(IAdminpaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Gets a list of all payments with associated customer details.
        /// </summary>
        /// <returns>A list of payment details.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AdminPaymentDetailDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<AdminPaymentDetailDto>>> GetAllPayments()
        {
            try
            {
                var payments = await _paymentService.GetAllPaymentsAsync();
                return Ok(payments);
            }
            catch (Exception ex)
            {
                // Log the exception (using a proper logging framework)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}