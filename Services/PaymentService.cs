using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IConfiguration _configuration;

        public PaymentService(IPaymentRepository paymentRepository, IConfiguration configuration)
        {
            _paymentRepository = paymentRepository;
            _configuration = configuration;
        }

        public async Task<IEnumerable<InvoiceDto>> GetPaymentsForUserAsync(int userId)
        {
            var payments = await _paymentRepository.GetPaymentsForUserAsync(userId);

            // Map the Payment entities to the InvoiceDto your frontend expects
            return payments.Select(p => new InvoiceDto
            {
                AppointmentId = p.AppointmentId,
                InvoiceNumber = $"INV-{p.PaymentId:D5}",
                ServiceName = $"Appointment - {p.Appointment.Type}", // Simple name
                Status = p.Status == PaymentStatus.Completed ? "paid" : "pending",
                Amount = p.Amount,
                Date = p.Appointment.DateTime, // Use the appointment date
                DueDate = p.Status == PaymentStatus.Pending ? p.Appointment.DateTime.AddDays(1) : (DateTime?)null,
                PaymentMethod = p.Status == PaymentStatus.Completed ? p.PaymentMethod.ToString() : null,
                InvoiceLink = p.InvoiceLink
            });
        }

        public async Task<string> CreateCheckoutSessionAsync(int appointmentId, int userId)
        {
            var payment = await _paymentRepository.GetByAppointmentIdAsync(appointmentId);

            // Validation
            if (payment == null)
                throw new Exception("Payment record not found.");
            if (payment.Appointment.UserId != userId)
                throw new Exception("Unauthorized to pay for this appointment.");
            if (payment.Status == PaymentStatus.Completed)
                throw new Exception("This payment has already been completed.");

            // These are the URLs Stripe will redirect to
            // Your frontend runs on port 3000
            var clientBaseUrl = "http://localhost:3000";
            var successUrl = $"{clientBaseUrl}/customer/payments?status=success";
            var cancelUrl = $"{clientBaseUrl}/customer/payments?status=cancelled";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd", // Change to "lkr" or your currency
                            UnitAmount = (long)(payment.Amount * 100), // Stripe expects value in cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Payment for Appointment #{payment.AppointmentId}",
                                Description = $"Service/Modification: {payment.Appointment.Type}"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                CustomerEmail = payment.Appointment.User.Email, // Pre-fill email
                Metadata = new Dictionary<string, string>
                {
                    // CRITICAL: Pass the AppointmentId so the webhook can find it
                    { "appointmentId", payment.AppointmentId.ToString() }
                }
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return session.Url; // This is the URL the frontend will redirect to
        }

        public async Task HandleStripeWebhookAsync(string json, string stripeSignature)
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            if (string.IsNullOrEmpty(webhookSecret))
                throw new Exception("Stripe WebhookSecret is not configured.");

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;

                    if (session?.PaymentStatus == "paid")
                    {
                        // Retrieve the appointmentId from metadata
                        session.Metadata.TryGetValue("appointmentId", out var appointmentIdStr);
                        if (int.TryParse(appointmentIdStr, out int appointmentId))
                        {
                            // Find the corresponding payment in DB
                            var payment = await _paymentRepository.GetByAppointmentIdAsync(appointmentId);
                            if (payment != null && payment.Status != PaymentStatus.Completed)
                            {
                                // Update the payment record
                                payment.Status = PaymentStatus.Completed;
                                payment.PaymentMethod = automobile_backend.Models.Entities.PaymentMethod.CreditCard; // Or get from session
                                payment.PaymentDateTime = DateTime.UtcNow;
                                // You can get the receipt URL if you expand the PaymentIntent
                                // payment.InvoiceLink = ...; 

                                await _paymentRepository.UpdateAsync(payment);
                            }
                        }
                    }
                }
                // Handle other event types (e.g., payment_failed) if needed
            }
            catch (StripeException ex)
            {
                // Handle invalid signature or other Stripe errors
                Console.WriteLine($"Stripe webhook error: {ex.Message}");
                throw;
            }
        }
    }
}