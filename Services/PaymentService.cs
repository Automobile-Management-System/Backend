using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // Added
using Stripe; // Added
using Stripe.Checkout; // Added
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IConfiguration _configuration; // Added

        public PaymentService(IPaymentRepository paymentRepository, IConfiguration configuration)
        {
            _paymentRepository = paymentRepository;
            _configuration = configuration; // Added
            
            // Set the Stripe API key statically from configuration
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<IEnumerable<InvoiceDto>> GetCustomerInvoicesAsync(int customerId)
        {
            // (Existing code is unchanged)
            var appointmentsQuery = _paymentRepository.GetCustomerAppointmentsWithDetails(customerId);

            var invoices = await appointmentsQuery
                .OrderByDescending(a => a.StartDateTime) 
                .Select(a => new InvoiceDto
                {
                    AppointmentId = a.AppointmentId,
                    InvoiceNumber = $"INV-{a.AppointmentId:D8}", 
                    ServiceName = string.Join(", ", a.AppointmentServices.Select(aps => aps.Service.ServiceName)),
                    Status = (a.Payment != null) ? "paid" : "pending",
                    Amount = (a.Payment != null) ? a.Payment.Amount : a.AppointmentServices.Sum(aps => aps.Service.Price),
                    Date = (a.Payment != null) ? a.Payment.PaymentDateTime : a.StartDateTime,
                    DueDate = (a.Payment == null) ? a.StartDateTime.AddDays(7) : null,
                    PaymentMethod = a.Payment != null ? a.Payment.PaymentMethod.ToString() : null, 
                    InvoiceLink = a.Payment != null ? a.Payment.InvoiceLink : null
                })
                .ToListAsync();

            return invoices;
        }

        // --- NEW METHOD ---
        public async Task<string> CreateCheckoutSessionAsync(int appointmentId, int userId, string successUrl, string cancelUrl)
        {
            // 1. Get appointment details and validate
            var appointment = await _paymentRepository.GetAppointmentForPaymentAsync(appointmentId);

            if (appointment == null)
            {
                throw new Exception("Appointment not found."); // Or a custom not found exception
            }
            if (appointment.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to pay for this appointment.");
            }
            if (appointment.Payment != null)
            {
                throw new InvalidOperationException("This appointment has already been paid.");
            }

            // 2. Calculate total amount
            var totalAmount = appointment.AppointmentServices.Sum(aps => aps.Service.Price);
            var serviceNames = string.Join(", ", appointment.AppointmentServices.Select(aps => aps.Service.ServiceName));

            // 3. Create Stripe Checkout Session options
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd", // Change currency if needed
                            UnitAmount = (long)(totalAmount * 100), // Stripe uses cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Payment for: {serviceNames}",
                                Description = $"Appointment ID: {appointment.AppointmentId}"
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                CustomerEmail = appointment.User.Email, // Pre-fill customer email
                // Add metadata to link the session back to our internal appointment ID
                Metadata = new Dictionary<string, string>
                {
                    { "appointmentId", appointment.AppointmentId.ToString() }
                }
            };

            // 4. Create and return the session URL
            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return session.Url;
        }

        // --- NEW METHOD ---
        public async Task<bool> FulfillOrderAsync(string sessionId)
        {
            try
            {
                var service = new SessionService();
                Session session = await service.GetAsync(sessionId, new SessionGetOptions { Expand = new List<string> { "payment_intent" } });

                if (session?.PaymentStatus != "paid")
                {
                    return false; // Payment not completed
                }

                int appointmentId = int.Parse(session.Metadata["appointmentId"]);

                // 1. Check if payment already recorded (idempotency)
                var appointment = await _paymentRepository.GetAppointmentForPaymentAsync(appointmentId);
                if (appointment.Payment != null)
                {
                    return true; // Already fulfilled
                }
                
                // 2. Create new Payment record
                var payment = new Payment
                {
                    Amount = (decimal)session.AmountTotal / 100,
                    PaymentMethod = Models.Entities.PaymentMethod.Stripe, // <-- ASSUMES you added 'Stripe' to your PaymentMethod enum
                    PaymentDateTime = DateTime.UtcNow,
                    InvoiceLink = session.PaymentIntentId, // Store Stripe Payment Intent ID
                    AppointmentId = appointmentId
                };

                // 3. Save to database
                await _paymentRepository.CreatePaymentRecordAsync(payment);
                return true;
            }
            catch (Exception ex)
            {
                // TODO: Log the error
                Console.WriteLine($"Error fulfilling order for session {sessionId}: {ex.Message}");
                return false;
            }
        }
    }
}