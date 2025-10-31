using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using automobile_backend.Repositories;
using Stripe; // This causes the conflict
using System; // Added for Exception
using System.Collections.Generic; // Added for IEnumerable
using System.Threading.Tasks; // Added for Task

// --- ADD THIS ALIAS ---
// We are telling the compiler that "Entities" in this file
// means this specific namespace.
using Entities = automobile_backend.Models.Entities;

namespace automobile_backend.Services
{
    public class AdminpaymentService : IAdminpaymentService
    {
        private readonly IAdminpaymentRepository _repository;
        private readonly IInvoiceService _invoiceService;

        public AdminpaymentService(IAdminpaymentRepository repository, IInvoiceService invoiceService)
        {
            _repository = repository;
            _invoiceService = invoiceService;
        }

        // --- UPDATE THE SIGNATURE HERE ---
        public async Task<(IEnumerable<AdminPaymentDetailDto> Items, int TotalCount)> GetAllPaymentsAsync(
            int pageNumber,
            int pageSize,
            string? search,
            Entities.PaymentStatus? status, // <-- Use the alias
            Entities.PaymentMethod? paymentMethod) // <-- Use the alias
        {
            return await _repository.GetAllPaymentsWithCustomerDetailsAsync(
                pageNumber,
                pageSize,
                search,
                status,
                paymentMethod
            );
        }

        // --- UPDATE THE SIGNATURE HERE ---
        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, Entities.PaymentStatus newStatus) // <-- Use the alias
        {
            string? invoiceUrl = null;

            // --- UPDATE THE COMPARISON HERE ---
            if (newStatus == Entities.PaymentStatus.Completed) // <-- Use the alias
            {
                try
                {
                    invoiceUrl = await _invoiceService.GenerateAndUploadInvoiceAsync(paymentId);
                }
                catch (Exception ex)
                {
                    // Log the exception
                }
            }

            return await _repository.UpdatePaymentStatusAsync(paymentId, newStatus, invoiceUrl);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _repository.GetTotalRevenueAsync();
        }

        // --- UPDATE THE ENUM ACCESS HERE ---
        public async Task<int> GetPendingCountAsync()
        {
            return await _repository.GetPaymentCountByStatusAsync(Entities.PaymentStatus.Pending); // <-- Use the alias
        }

        // --- UPDATE THE ENUM ACCESS HERE ---
        public async Task<int> GetCompletedCountAsync()
        {
            return await _repository.GetPaymentCountByStatusAsync(Entities.PaymentStatus.Completed); // <-- Use the alias
        }

        // --- UPDATE THE ENUM ACCESS HERE ---
        public async Task<int> GetFailedCountAsync()
        {
            return await _repository.GetPaymentCountByStatusAsync(Entities.PaymentStatus.Failed); // <-- Use the alias
        }
    }
}