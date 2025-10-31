using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using automobile_backend.Repositories;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// --- ADD THESE 2 USING DIRECTIVES ---
using automobile_backend.Services.ReportTemplates; // <-- This one finds PaymentsReportDocument
using QuestPDF.Fluent; // <-- This one finds .GeneratePdf()

// --- ADD THIS ALIAS ---
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

        public async Task<(IEnumerable<AdminPaymentDetailDto> Items, int TotalCount)> GetAllPaymentsAsync(
            int pageNumber,
            int pageSize,
            string? search,
            Entities.PaymentStatus? status,
            Entities.PaymentMethod? paymentMethod)
        {
            return await _repository.GetAllPaymentsWithCustomerDetailsAsync(
                pageNumber,
                pageSize,
                search,
                status,
                paymentMethod
            );
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, Entities.PaymentStatus newStatus)
        {
            string? invoiceUrl = null;

            if (newStatus == Entities.PaymentStatus.Completed)
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


        public async Task<(byte[] pdfBytes, string fileName)> GeneratePaymentsReportAsync(
            string? search,
            Entities.PaymentStatus? status,
            Entities.PaymentMethod? paymentMethod)
        {
            // 1. Get ALL filtered data from the new repo method
            var payments = await _repository.GetAllFilteredPaymentsForReportAsync(search, status, paymentMethod);

            // 2. Create the PDF document (This now works)
            var document = new PaymentsReportDocument(payments, search, status, paymentMethod);

            // 3. Generate the PDF bytes (This now works)
            byte[] pdfBytes = document.GeneratePdf();

            string fileName = $"Payments_Report_{DateTime.Now:yyyyMMdd_HHmm}.pdf";

            return (pdfBytes, fileName);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _repository.GetTotalRevenueAsync();
        }

        public async Task<int> GetPendingCountAsync()
        {
            return await _repository.GetPaymentCountByStatusAsync(Entities.PaymentStatus.Pending);
        }

        public async Task<int> GetCompletedCountAsync()
        {
            return await _repository.GetPaymentCountByStatusAsync(Entities.PaymentStatus.Completed);
        }

        public async Task<int> GetFailedCountAsync()
        {
            return await _repository.GetPaymentCountByStatusAsync(Entities.PaymentStatus.Failed);
        }
    }
}