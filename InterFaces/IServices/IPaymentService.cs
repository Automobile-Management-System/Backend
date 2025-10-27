using automobile_backend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IPaymentService
    {
        /// <summary>
        /// Gets a formatted list of all invoices (paid and pending) for a customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer (user).</param>
        /// <returns>A list of InvoiceDto objects.</returns>
        Task<IEnumerable<InvoiceDto>> GetCustomerInvoicesAsync(int customerId);

        /// <summary>
        /// Creates a Stripe Checkout Session for a given appointment.
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment to pay for.</param>
        /// <param name="userId">The ID of the user making the request (for validation).</param>
        /// <param name="successUrl">The URL to redirect to on payment success.</param>
        /// <param name="cancelUrl">The URL to redirect to on payment cancellation.</param>
        /// <returns>The URL for the Stripe Checkout page.</returns>
        Task<string> CreateCheckoutSessionAsync(int appointmentId, int userId, string successUrl, string cancelUrl);

        /// <summary>
        /// Fulfills an order after a successful Stripe payment.
        /// Called by the Stripe webhook.
        /// </summary>
        /// <param name="sessionId">The Stripe Checkout Session ID.</param>
        /// <returns>True if fulfillment was successful.</returns>
        Task<bool> FulfillOrderAsync(string sessionId);
    }
}