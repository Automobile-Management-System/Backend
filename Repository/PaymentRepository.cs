using automobile_backend.Data; // For ApplicationDbContext
using automobile_backend.InterFaces.IRepository; // For IPaymentRepository
using automobile_backend.Models.Entities; // For Appointment AND your enums
using Microsoft.EntityFrameworkCore;
using System.Linq;

// DELETE this line: using automobile_backend.Models.Enums; 

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
                .Where(a => a.UserId == userId && a.Status != AppointmentStatus.Cancelled) // This line will now work
                .AsQueryable();
        }
    }
}