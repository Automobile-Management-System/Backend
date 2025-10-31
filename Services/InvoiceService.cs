using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Entities;
using automobile_backend.Services.InvoiceTemplates; // We will create this
using QuestPDF.Fluent;
using System;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IPaymentRepository _paymentRepository;

        public InvoiceService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
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
    }
}
