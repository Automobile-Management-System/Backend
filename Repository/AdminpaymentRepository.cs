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

        public async Task<IEnumerable<AdminPaymentDetailDto>> GetAllPaymentsWithCustomerDetailsAsync()
        {
            var paymentDetails = await _context.Payments
                // Include the path to the User for customer details
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.User)
                // Include the path to Service names
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.AppointmentServices)
                        .ThenInclude(aps => aps.Service)
                // Include the path to Modification titles
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.ModificationRequests)

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
                    AppointmentType = p.Appointment.Type.ToString(), // Get the enum string

                    // Customer Details
                    CustomerId = p.Appointment.User.UserId,
                    CustomerFirstName = p.Appointment.User.FirstName,
                    CustomerLastName = p.Appointment.User.LastName,
                    CustomerEmail = p.Appointment.User.Email,
                    CustomerPhoneNumber = p.Appointment.User.PhoneNumber,

                    // NEW: Conditionally populate ServiceNames or ModificationTitles
                    ServiceNames = (p.Appointment.Type == Models.Entities.Type.Service)
                        ? p.Appointment.AppointmentServices.Select(aps => aps.Service.ServiceName)
                        : new List<string>(), // Return empty list if not a Service type

                    ModificationTitles = (p.Appointment.Type == Models.Entities.Type.Modifications)
                        ? p.Appointment.ModificationRequests.Select(mr => mr.Title)
                        : new List<string>() // Return empty list if not a Modification type
                })
                .OrderByDescending(p => p.PaymentDateTime)
                .ToListAsync();

            return paymentDetails;
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