using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace automobile_backend.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize] // Protect all endpoints by default
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        // --- NEW ---
        private readonly IInvoiceService _invoiceService;

        // --- UPDATED ---
        public PaymentController(IPaymentService paymentService, IInvoiceService invoiceService)
        {
            _paymentService = paymentService;
            _invoiceService = invoiceService; // Inject new service
        }

        // GET /api/payments
        // Gets all payments for the currently logged-in user
        [HttpGet]
        public async Task<IActionResult> GetUserPayments()
        {
            // Get user ID from the JWT token claim
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            try
            {
                var payments = await _paymentService.GetPaymentsForUserAsync(userId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/payments/create-checkout-session
        // Creates a new Stripe checkout session
        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            try
            {
                var sessionUrl = await _paymentService.CreateCheckoutSessionAsync(dto.AppointmentId, userId);
                // Return the URL in the format your frontend expects
                return Ok(new { url = sessionUrl });
            }
            catch (Exception ex)
            {
                // Return a specific error message
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/payments/webhook
        // This endpoint is called BY STRIPE, not your frontend.
        // It must be publicly accessible.
        [HttpPost("webhook")]
        [AllowAnonymous] // Remove authorization for the webhook
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"];

            try
            {
                await _paymentService.HandleStripeWebhookAsync(json, stripeSignature);
                return Ok();
            }
            catch (StripeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- NEW ---
        // GET /api/payments/{appointmentId}/invoice
        // Generates and returns a PDF invoice
        [HttpGet("{appointmentId}/invoice")]
        public async Task<IActionResult> GetInvoice(int appointmentId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            try
            {
                // Call the service to generate the PDF
                var pdfBytes = await _invoiceService.GenerateInvoiceAsync(appointmentId, userId);
                
                // Return the PDF as a file download
                string invoiceNumber = $"INV-{appointmentId:D5}";
                return File(pdfBytes, "application/pdf", $"{invoiceNumber}.pdf");
            }
            catch (Exception ex)
            {
                // Handle errors like "Unauthorized" or "Not Found"
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}