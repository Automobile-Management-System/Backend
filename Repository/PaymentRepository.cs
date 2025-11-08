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

        public async Task<Payment?> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.User) 
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);
        }

        public async Task<Payment> UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        // --- THIS METHOD IS UPDATED ---
        public async Task<IEnumerable<Payment>> GetPaymentsForUserAsync(int userId)
        {
            return await _context.Payments
                .Include(p => p.Appointment) // Get the appointment
                    .ThenInclude(a => a.AppointmentServices) // THEN Include the join table
                    .ThenInclude(aps => aps.Service)         // THEN Include the Service from the join table
                .Include(p => p.Appointment) // Get the appointment again
                    .ThenInclude(a => a.ModificationRequests) // THEN Include its modification requests
                .Where(p => p.Appointment.UserId == userId)
                .OrderByDescending(p => p.Appointment.DateTime)
                .ToListAsync();
        }

        public Task<Payment?> GetPaymentForInvoiceAsync(int paymentId)
        {
            throw new NotImplementedException();
        }

        public Task<Payment> CreateAsync(Payment payment)
        {
            throw new NotImplementedException();
        }
    }
}