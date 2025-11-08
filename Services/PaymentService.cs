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
using automobile_backend.Models.Entities;

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

        // --- THIS METHOD IS UPDATED ---
        public async Task<IEnumerable<InvoiceDto>> GetPaymentsForUserAsync(int userId)
        {
            // This now returns payments with the extra related data
            var payments = await _paymentRepository.GetPaymentsForUserAsync(userId); 

            return payments.Select(p => new InvoiceDto
            {
                AppointmentId = p.AppointmentId,
                InvoiceNumber = $"INV-{p.PaymentId:D5}",
                
                // --- THIS IS THE CHANGE ---
                // We now call our helper function to get the real name(s)
                ServiceName = GetServiceDescription(p.Appointment),
                // --- END CHANGE ---
                
                Status = p.Status == PaymentStatus.Completed ? "paid" : "pending",
                Amount = p.Amount,
                Date = p.Appointment.DateTime, 
                DueDate = p.Status == PaymentStatus.Pending ? p.Appointment.DateTime.AddDays(1) : (DateTime?)null,
                PaymentMethod = p.Status == PaymentStatus.Completed ? p.PaymentMethod.ToString() : null,
                InvoiceLink = p.InvoiceLink 
            });
        }
        
        // --- NEW HELPER METHOD ---
        // This logic builds the service name based on the data we fetched
        private string GetServiceDescription(Appointment appointment)
        {
            // Check if it's a Service appointment
            if (appointment.Type == Models.Entities.Type.Service)
            {
                if (appointment.AppointmentServices != null && appointment.AppointmentServices.Any())
                {
                    // Join all linked service names, e.g., "Oil Change, Tire Rotation"
                    return string.Join(", ", appointment.AppointmentServices.Select(aps => aps.Service?.ServiceName ?? "Service"));
                }
                return "General Service"; // Fallback
            }

            // Check if it's a Modification appointment
            if (appointment.Type == Models.Entities.Type.Modifications)
            {
                if (appointment.ModificationRequests != null && appointment.ModificationRequests.Any())
                {
                    // Use the title of the first modification request
                    return appointment.ModificationRequests.First().Title;
                }
                return "Modification Request"; // Fallback
            }

            return "Unknown Service";
        }
        // --- END NEW HELPER METHOD ---

        
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

            // --- THIS IS THE CHANGE ---
            // Get the detailed service name for the Stripe page
            var serviceDescription = GetServiceDescription(payment.Appointment);
            // --- END CHANGE ---

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
                                // --- THIS IS THE CHANGE ---
                                // Use the detailed name on the Stripe checkout page
                                Name = serviceDescription,
                                Description = $"Payment for Appointment #{payment.AppointmentId}"
                                // --- END CHANGE ---
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

        public async Task HandleStripeWebhookAsync(string json, string? stripeSignature)
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            if (string.IsNullOrEmpty(webhookSecret))
                throw new Exception("Stripe WebhookSecret is not configured.");

            if (string.IsNullOrEmpty(stripeSignature))
                throw new ArgumentException("Stripe-Signature header is missing.", nameof(stripeSignature));

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret, throwOnApiVersionMismatch: false);

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;

                    if (session?.PaymentStatus == "paid")
                    {
                        session.Metadata.TryGetValue("appointmentId", out var appointmentIdStr);
                        if (int.TryParse(appointmentIdStr, out int appointmentId))
                        {
                            // We must use GetByAppointmentIdAsync here, as GetPaymentsForUserAsync needs a user ID
                            // But we need to fetch the *full* data for the PDF
                            // Let's create a new repo method or modify GetByAppointmentIdAsync
                            
                            // For simplicity, let's update GetByAppointmentIdAsync to fetch everything needed for the PDF
                            // (We already modified the other one, let's update this one too)
                            // **UPDATE: GetByAppointmentIdAsync in your repo MUST also be updated**
                            
                            var payment = await _paymentRepository.GetByAppointmentIdAsync(appointmentId); // <-- This needs to be the full data
                            
                            if (payment == null)
                            {
                                 Console.WriteLine($"Webhook error: Payment for appointment {appointmentId} not found.");
                                 return; // Stop processing
                            }

                            if (payment.Status != PaymentStatus.Completed)
                            {
                                payment.Status = PaymentStatus.Completed; 
                                payment.PaymentMethod = Models.Entities.PaymentMethod.CreditCard; 
                                payment.PaymentDateTime = DateTime.UtcNow;

                                var document = new InvoiceDocument(payment);
                                byte[] pdfBytes = document.GeneratePdf();
                                string fileName = $"invoices/INV-{payment.PaymentId:D5}.pdf";

                                string downloadUrl = await _cloudStorageService.UploadFileAsync(
                                    pdfBytes, 
                                    fileName, 
                                    "application/pdf"
                                );

                                payment.InvoiceLink = downloadUrl;
                                
                                await _paymentRepository.UpdateAsync(payment);
                            }
                        }
                    }
                }
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"Stripe webhook signature/event error: {ex.Message}"); 
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing webhook (PDF/Firebase?): {ex.Message}");
                throw;
            }
        }
    }
}