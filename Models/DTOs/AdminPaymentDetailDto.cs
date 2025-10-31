// In automobile_backend/Models/DTOs/AdminPaymentDetailDto.cs

namespace automobile_backend.Models.DTOs
{
    public class AdminPaymentDetailDto
    {
        // --- Payment Details ---
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDateTime { get; set; }
        public string? InvoiceLink { get; set; }

        // --- Related Appointment Info ---
        public int AppointmentId { get; set; }

        // --- Customer Details ---
        public int CustomerId { get; set; }
        public string CustomerFirstName { get; set; } = string.Empty;
        public string CustomerLastName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhoneNumber { get; set; }

 
        public string AppointmentType { get; set; } = string.Empty;

 
        public IEnumerable<string> ServiceNames { get; set; } = new List<string>();


        public IEnumerable<string> ModificationTitles { get; set; } = new List<string>();
    }
}