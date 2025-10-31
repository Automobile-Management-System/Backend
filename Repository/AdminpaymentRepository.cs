// In automobile_backend/Repositories/AdminpaymentRepository.cs

using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities; // Make sure this is included for the 'Type' enum
using Microsoft.EntityFrameworkCore;

namespace automobile_backend.Repositories
{
    public class AdminpaymentRepository : IAdminpaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminpaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<AdminPaymentDetailDto> Items, int TotalCount)> GetAllPaymentsWithCustomerDetailsAsync(int pageNumber, int pageSize)
        {
            // 1. Build the base query with all includes
            var query = _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.User)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.AppointmentServices)
                        .ThenInclude(aps => aps.Service)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.ModificationRequests);

            // 2. Get the total count *before* pagination
            var totalCount = await query.CountAsync();

            // 3. Project, Order, Paginate, and Execute
            var paymentDetails = await query
                .Select(p => new AdminPaymentDetailDto
                {
                    // Payment Details
                    PaymentId = p.PaymentId,
                    Amount = p.Amount,
                    Status = p.Status.ToString(),
                    PaymentMethod = p.PaymentMethod.ToString(),
                    PaymentDateTime = p.PaymentDateTime,
                    InvoiceLink = p.InvoiceLink,

                    // Appointment Info
                    AppointmentId = p.AppointmentId,
                    AppointmentType = p.Appointment.Type.ToString(),

                    // Customer Details
                    CustomerId = p.Appointment.User.UserId,
                    CustomerFirstName = p.Appointment.User.FirstName,
                    CustomerLastName = p.Appointment.User.LastName,
                    CustomerEmail = p.Appointment.User.Email,
                    CustomerPhoneNumber = p.Appointment.User.PhoneNumber,

                    // Conditional population
                    ServiceNames = (p.Appointment.Type == Models.Entities.Type.Service)
                        ? p.Appointment.AppointmentServices.Select(aps => aps.Service.ServiceName)
                        : new List<string>(),

                    ModificationTitles = (p.Appointment.Type == Models.Entities.Type.Modifications)
                        ? p.Appointment.ModificationRequests.Select(mr => mr.Title)
                        : new List<string>()
                })
                .OrderByDescending(p => p.PaymentDateTime) // Order must be applied *before* Skip/Take
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 4. Return the tuple
            return (paymentDetails, totalCount);
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, PaymentStatus newStatus)
        {

            var payment = await _context.Payments.FindAsync(paymentId);

            if (payment == null)
            {

                return false;
            }

            payment.Status = newStatus;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}