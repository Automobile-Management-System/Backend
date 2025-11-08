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


        // --- THIS METHOD IS UPDATED ---
        public async Task<Payment?> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.User)
                // --- ADD THESE INCLUDES ---
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.AppointmentServices)
                    .ThenInclude(aps => aps.Service)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.ModificationRequests)
                // --- END OF ADDITIONS ---
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);
        }

        public async Task<Payment> UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        
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


        //  Eshan
        public async Task<Payment?> GetPaymentForInvoiceAsync(int paymentId)
        {
            return await _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.User)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.AppointmentServices)
                    .ThenInclude(aps => aps.Service)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.ModificationRequests)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }



        //  Eshan
        public async Task<Payment> CreateAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }
    }
}