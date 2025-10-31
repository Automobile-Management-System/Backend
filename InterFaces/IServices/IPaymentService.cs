using automobile_backend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IPaymentService
    {
        // UPDATED - Now takes a userId and returns the InvoiceDto list
        Task<IEnumerable<InvoiceDto>> GetPaymentsForUserAsync(int userId);

        // NEW - Creates the Stripe session
        Task<string> CreateCheckoutSessionAsync(int appointmentId, int userId);

        // NEW - Handles the success event from Stripe
        Task HandleStripeWebhookAsync(string json, string stripeSignature);
    }
}