using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Entities;
using automobile_backend.Services.InvoiceTemplates; // We will create this
using QuestPDF.Fluent;
using automobile_backend.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICloudStorageService _cloudStorageService;

        public InvoiceService(IPaymentRepository paymentRepository, ICloudStorageService cloudStorageService)
        {
            _paymentRepository = paymentRepository;
            _cloudStorageService = cloudStorageService;
        }

        public async Task<byte[]> GenerateInvoiceAsync(int appointmentId, int userId)
        {
            // 1. Get payment data
            var payment = await _paymentRepository.GetByAppointmentIdAsync(appointmentId);

            // 2. Validation
            if (payment == null)
                throw new Exception("Invoice not found.");

            if (payment.Appointment.UserId != userId)
                throw new Exception("You are not authorized to view this invoice.");

            if (payment.Status != PaymentStatus.Completed)
                throw new Exception("Payment for this invoice is not complete.");

            // 3. Create PDF
            var document = new InvoiceDocument(payment); // Pass data to template
            byte[] pdfBytes = document.GeneratePdf();

            return pdfBytes;
        }

        public async Task<string?> GenerateAndUploadInvoiceAsync(int paymentId)
        {
            // 1. Get payment data using the new repo method
            var payment = await _paymentRepository.GetPaymentForInvoiceAsync(paymentId);

            // 2. Validation (no user auth check needed, this is a system call)
            if (payment == null)
            {
                // Log this error
                return null;
            }

            // 3. Create PDF
            var document = new InvoiceDocument(payment);
            byte[] pdfBytes = document.GeneratePdf();

            // 4. Upload to Firebase
            // Creates a unique file name, e.g., "invoices/INV-00123-guid.pdf"
            string fileName = $"invoices/INV-{payment.PaymentId:D5}-{Guid.NewGuid()}.pdf";

            // The CloudStorageService handles the upload and returns the public URL
            string fileUrl = await _cloudStorageService.UploadFileAsync(pdfBytes, fileName, "application/pdf");

            // 5. Return the URL
            return fileUrl;
        }
    }
}
