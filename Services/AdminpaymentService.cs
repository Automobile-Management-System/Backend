using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using automobile_backend.Repositories;
using Stripe;

namespace automobile_backend.Services
{
    public class AdminpaymentService : IAdminpaymentService
    {
        private readonly IAdminpaymentRepository _repository;
        private readonly IInvoiceService _invoiceService;

        public AdminpaymentService(IAdminpaymentRepository repository , IInvoiceService invoiceService)
        {
            _repository = repository;
            _invoiceService = invoiceService;
        }

        public async Task<(IEnumerable<AdminPaymentDetailDto> Items, int TotalCount)> GetAllPaymentsAsync(int pageNumber, int pageSize)
        {
            return await _repository.GetAllPaymentsWithCustomerDetailsAsync(pageNumber, pageSize);
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, PaymentStatus newStatus)
        {
            string? invoiceUrl = null;

            // --- ADD THIS LOGIC ---
            // If the admin is marking the payment as 'Completed',
            // generate and upload the invoice.
            if (newStatus == PaymentStatus.Completed)
            {
                try
                {
                    // This generates, uploads, and returns the public Firebase URL
                    invoiceUrl = await _invoiceService.GenerateAndUploadInvoiceAsync(paymentId);
                }
                catch (Exception ex)
                {
                    // Log the exception (e.g., "Failed to generate/upload invoice for PaymentId {paymentId}")
                    // We'll proceed with the status update even if invoice generation fails,
                    // but the InvoiceLink will remain null.
                }
            }
            // --- END ADDED LOGIC ---

            // Pass the new status AND the (possibly null) invoice URL to the repository
            return await _repository.UpdatePaymentStatusAsync(paymentId, newStatus, invoiceUrl);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            // Business logic: Revenue is defined as the sum of *Completed* payments.
            return await _repository.GetTotalRevenueAsync();
        }

        public async Task<int> GetPendingCountAsync()
        {
            return await _repository.GetPaymentCountByStatusAsync(PaymentStatus.Pending);
        }

        public async Task<int> GetCompletedCountAsync()
        {
            return await _repository.GetPaymentCountByStatusAsync(PaymentStatus.Completed);
        }

        public async Task<int> GetFailedCountAsync()
        {
            return await _repository.GetPaymentCountByStatusAsync(PaymentStatus.Failed);
        }

    }
}