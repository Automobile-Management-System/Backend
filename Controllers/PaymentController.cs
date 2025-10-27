using automobile_backend.InterFaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // Added
using System.IO; // Added
using Stripe; // Added

namespace automobile_backend.Controllers
{
    // A simple DTO to receive the AppointmentId from the frontend
    public class CheckoutRequestDto
    {
        public int AppointmentId { get; set; }
    }

    [ApiController]
    [Route("api/payments")]
    [Authorize] // Secure this endpoint, only logged-in users can access
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration; // Added

        public PaymentController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration; // Added
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerInvoices()
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized("User ID not found or invalid in token.");
                }

                var invoices = await _paymentService.GetCustomerInvoicesAsync(userId);
                return Ok(invoices);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // --- NEW ENDPOINT ---
        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutRequestDto request)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                {
                    return Unauthorized("User ID not found or invalid in token.");
                }

                // --- IMPORTANT ---
                // Define your frontend success/cancel routes here
                // These are placeholders! Change them to your actual React routes.
                var baseUrl = "http://localhost:3000"; // Your React app's URL
                var successUrl = $"{baseUrl}/dashboard/payments?success=true";
                var cancelUrl = $"{baseUrl}/dashboard/payments?canceled=true";

                var sessionUrl = await _paymentService.CreateCheckoutSessionAsync(request.AppointmentId, userId, successUrl, cancelUrl);
                
                // Return the URL for the frontend to redirect to
                return Ok(new { Url = sessionUrl });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                // TODO: Log the error
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        // --- NEW WEBHOOK ENDPOINT ---
        [HttpPost("stripe-webhook")]
        [AllowAnonymous] // This endpoint must be public for Stripe to reach it
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"];
            var webhookSecret = _configuration["Stripe:WebhookSecret"];

            if (string.IsNullOrEmpty(webhookSecret))
            {
                 // TODO: Log this critical configuration error
                return BadRequest("Stripe webhook secret not configured.");
            }

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);

                // Handle the checkout.session.completed event
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    if (session != null)
                    {
                        await _paymentService.FulfillOrderAsync(session.Id);
                    }
                }
                else
                {
                    // Handle other event types if needed
                    Console.WriteLine($"Unhandled Stripe event type: {stripeEvent.Type}");
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                // Invalid signature
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // TODO: Log the error
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}