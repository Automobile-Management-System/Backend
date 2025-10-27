using automobile_backend.Data;
using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks; // Added

namespace automobile_backend.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Appointment> GetCustomerAppointmentsWithDetails(int userId)
        {
            // Build the query to get all appointments for the customer.
            // We include all related data needed to build the InvoiceDto.
            return _context.Appointments
                .Include(a => a.Payment) // The one-to-one Payment (if it exists)
                .Include(a => a.AppointmentServices) // The join table
                    .ThenInclude(aps => aps.Service) // The Service entity via the join table
                .Where(a => a.UserId == userId && a.Status != AppointmentStatus.Cancelled)
                .AsQueryable();
        }
        
        // --- NEW METHOD ---
        public async Task<Appointment> GetAppointmentForPaymentAsync(int appointmentId)
        {
            // Get a single appointment, including the User (for email) and
            // service details (for price and name).
            return await _context.Appointments
                .Include(a => a.Payment)
                .Include(a => a.User) // Include User to get customer email for Stripe
                .Include(a => a.AppointmentServices)
                    .ThenInclude(aps => aps.Service)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        }

        // --- NEW METHOD ---
        public async Task CreatePaymentRecordAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }
    }
}