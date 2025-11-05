using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace automobile_backend.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments.ToListAsync();
        }

        // UPDATED - Eagerly loads Appointment and User for validation
        public async Task<Payment?> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.User) // Include User for email
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);
        }

        public async Task<Payment> UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        // NEW - Implementation for getting user-specific payments
        public async Task<IEnumerable<Payment>> GetPaymentsForUserAsync(int userId)
        {
            return await _context.Payments
                .Include(p => p.Appointment) // Include Appointment details
                .Where(p => p.Appointment.UserId == userId)
                .OrderByDescending(p => p.Appointment.DateTime)
                .ToListAsync();
        }

        public async Task<Payment?> GetPaymentForInvoiceAsync(int paymentId)
        {
            // Eagerly load the data needed by your InvoiceDocument template
            return await _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }

        // NEW - Implementation for creating a new payment record
        public async Task<Payment> CreateAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }
    }
}