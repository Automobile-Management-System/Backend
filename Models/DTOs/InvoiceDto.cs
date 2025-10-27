namespace automobile_backend.Models.DTOs
{
    public class InvoiceDto
    {
        public int AppointmentId { get; set; }
        public string InvoiceNumber { get; set; }
        public string ServiceName { get; set; }
        public string Status { get; set; } // "paid" or "pending"
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public DateTime? DueDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? InvoiceLink { get; set; }
    }
}