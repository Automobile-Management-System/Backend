using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Services.InvoiceTemplates; 
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent; 
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using automobile_backend.Models.Entities; // <-- THE CRITICAL FIX IS HERE

namespace automobile_backend.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IConfiguration _configuration;
        private readonly ICloudStorageService _cloudStorageService;

        public PaymentService(
            IPaymentRepository paymentRepository, 
            IConfiguration configuration, 
            ICloudStorageService cloudStorageService)
        {
            _paymentRepository = paymentRepository;
            _configuration = configuration;
            _cloudStorageService = cloudStorageService; 
        }

        public async Task<IEnumerable<InvoiceDto>> GetPaymentsForUserAsync(int userId)
        {
            var payments = await _paymentRepository.GetPaymentsForUserAsync(userId);

            // This DTO mapping correctly handles the status
            return payments.Select(p => new InvoiceDto
            {
                AppointmentId = p.AppointmentId,
                InvoiceNumber = $"INV-{p.PaymentId:D5}",
                ServiceName = $"Appointment - {p.Appointment.Type}", 
                Status = p.Status == PaymentStatus.Completed ? "paid" : "pending",
                Amount = p.Amount,
                Date = p.Appointment.DateTime, 
                DueDate = p.Status == PaymentStatus.Pending ? p.Appointment.DateTime.AddDays(1) : (DateTime?)null,
                PaymentMethod = p.Status == PaymentStatus.Completed ? p.PaymentMethod.ToString() : null,
                InvoiceLink = p.InvoiceLink 
            });
        }
        
        public async Task<string> CreateCheckoutSessionAsync(int appointmentId, int userId)
        {
            var payment = await _paymentRepository.GetByAppointmentIdAsync(appointmentId);

            if (payment == null)
                throw new Exception("Payment record not found.");
            if (payment.Appointment.UserId != userId)
                throw new Exception("Unauthorized to pay for this appointment.");
            if (payment.Status == PaymentStatus.Completed)
                throw new Exception("This payment has already been completed.");

            var clientBaseUrl = "http://localhost:3000";
            var successUrl = $"{clientBaseUrl}/customer/payments?status=success";
            var cancelUrl = $"{clientBaseUrl}/customer/payments"; 

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "lkr", 
                            UnitAmount = (long)(payment.Amount * 100), 
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
                CustomerEmail = payment.Appointment.User.Email, 
                Metadata = new Dictionary<string, string>
                {
                    { "appointmentId", payment.AppointmentId.ToString() }
                }
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return session.Url; 
        }

        // This method updates the status in the database
        public async Task HandleStripeWebhookAsync(string json, string? stripeSignature)
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            if (string.IsNullOrEmpty(webhookSecret))
                throw new Exception("Stripe WebhookSecret is not configured.");

            if (string.IsNullOrEmpty(stripeSignature))
                throw new ArgumentException("Stripe-Signature header is missing.", nameof(stripeSignature));

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;

                    if (session?.PaymentStatus == "paid")
                    {
                        session.Metadata.TryGetValue("appointmentId", out var appointmentIdStr);
                        if (int.TryParse(appointmentIdStr, out int appointmentId))
                        {
                            var payment = await _paymentRepository.GetByAppointmentIdAsync(appointmentId);
                            if (payment != null && payment.Status != PaymentStatus.Completed)
                            {
                                // 1. Update Payment Status to Completed (uses the Enum)
                                payment.Status = PaymentStatus.Completed; 
                                payment.PaymentMethod = automobile_backend.Models.Entities.PaymentMethod.CreditCard; 
                                payment.PaymentDateTime = DateTime.UtcNow;

                                // 2. Generate PDF
                                var document = new InvoiceDocument(payment);
                                byte[] pdfBytes = document.GeneratePdf();
                                string fileName = $"invoices/INV-{payment.PaymentId:D5}.pdf";

                                // 3. Upload to Firebase Storage
                                string downloadUrl = await _cloudStorageService.UploadFileAsync(
                                    pdfBytes, 
                                    fileName, 
                                    "application/pdf"
                                );

                                // 4. Save the public URL
                                payment.InvoiceLink = downloadUrl;
                                
                                // 5. Update database with all changes
                                await _paymentRepository.UpdateAsync(payment);
                            }
                        }
                    }
                }
            }
            catch (StripeException ex)
            {
                // Check backend console for these errors
                Console.WriteLine($"Stripe webhook signature/event error: {ex.Message}"); 
                throw;
            }
            catch (Exception ex)
            {
                // Check backend console for PDF/Firebase errors
                Console.WriteLine($"Error processing webhook (PDF/Firebase?): {ex.Message}");
                throw;
            }
        }
    }
}