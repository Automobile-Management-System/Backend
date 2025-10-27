using automobile_backend.InterFaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace automobile_backend.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize] // Secure this endpoint, only logged-in users can access
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerInvoices()
        {
            try
            {
                // Get the logged-in user's ID from the JWT token
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Validate the ID from the token
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized("User ID not found or invalid in token.");
                }

                var invoices = await _paymentService.GetCustomerInvoicesAsync(userId);
                return Ok(invoices);
            }
            catch (System.Exception ex)
            {
                // TODO: Log the error
                return StatusCode(500, "Internal server error");
            }
        }
    }
}