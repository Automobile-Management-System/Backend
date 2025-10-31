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

        public async Task<(IEnumerable<AdminPaymentDetailDto> Items, int TotalCount)> GetAllPaymentsWithCustomerDetailsAsync(
                int pageNumber,
                int pageSize,
                string? search,
                PaymentStatus? status,
                PaymentMethod? paymentMethod)
        {
            // 1. Build the base query with all includes
            var query = _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.User)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.AppointmentServices)
                        .ThenInclude(aps => aps.Service)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.ModificationRequests)
                .AsQueryable(); // Start building the query

            // 2. Apply Search Filter (if provided)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower().Trim();
                query = query.Where(p =>
                    (p.Appointment.User.FirstName + " " + p.Appointment.User.LastName).ToLower().Contains(searchTerm) ||
                    p.Appointment.User.Email.ToLower().Contains(searchTerm) ||
                    p.Appointment.User.PhoneNumber.Contains(searchTerm)
                );
            }

            // 3. Apply Status Filter (if provided)
            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            // 4. Apply Payment Method Filter (if provided)
            if (paymentMethod.HasValue)
            {
                query = query.Where(p => p.PaymentMethod == paymentMethod.Value);
            }

            // 5. Get the total count *after* all filters are applied
            var totalCount = await query.CountAsync();

            // 6. Project, Order, Paginate, and Execute
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

            // 7. Return the tuple
            return (paymentDetails, totalCount);
        }


        public async Task<IEnumerable<AdminPaymentDetailDto>> GetAllFilteredPaymentsForReportAsync(
            string? search,
            PaymentStatus? status,
            PaymentMethod? paymentMethod)
        {
            // 1. Build the base query with all includes
            var query = _context.Payments
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.User)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.AppointmentServices)
                        .ThenInclude(aps => aps.Service)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.ModificationRequests)
                .AsQueryable();

            // 2. Apply Search Filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower().Trim();
                query = query.Where(p =>
                    (p.Appointment.User.FirstName + " " + p.Appointment.User.LastName).ToLower().Contains(searchTerm) ||
                    p.Appointment.User.Email.ToLower().Contains(searchTerm) ||
                    p.Appointment.User.PhoneNumber.Contains(searchTerm)
                );
            }

            // 3. Apply Status Filter
            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            // 4. Apply Payment Method Filter
            if (paymentMethod.HasValue)
            {
                query = query.Where(p => p.PaymentMethod == paymentMethod.Value);
            }

            // 5. Project, Order, and Execute (NO PAGINATION)
            var paymentDetails = await query
                .Select(p => new AdminPaymentDetailDto
                {
                    // (Same projection as your other method)
                    PaymentId = p.PaymentId,
                    Amount = p.Amount,
                    Status = p.Status.ToString(),
                    PaymentMethod = p.PaymentMethod.ToString(),
                    PaymentDateTime = p.PaymentDateTime,
                    InvoiceLink = p.InvoiceLink,
                    AppointmentId = p.AppointmentId,
                    AppointmentType = p.Appointment.Type.ToString(),
                    CustomerId = p.Appointment.User.UserId,
                    CustomerFirstName = p.Appointment.User.FirstName,
                    CustomerLastName = p.Appointment.User.LastName,
                    CustomerEmail = p.Appointment.User.Email,
                    CustomerPhoneNumber = p.Appointment.User.PhoneNumber,
                    ServiceNames = (p.Appointment.Type == Models.Entities.Type.Service)
                        ? p.Appointment.AppointmentServices.Select(aps => aps.Service.ServiceName)
                        : new List<string>(),
                    ModificationTitles = (p.Appointment.Type == Models.Entities.Type.Modifications)
                        ? p.Appointment.ModificationRequests.Select(mr => mr.Title)
                        : new List<string>()
                })
                .OrderByDescending(p => p.PaymentDateTime)
                .ToListAsync();

            return paymentDetails;
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, PaymentStatus newStatus, string? invoiceUrl = null)
        {
            var payment = await _context.Payments.FindAsync(paymentId);

            if (payment == null)
            {
                return false;
            }

            payment.Status = newStatus;

            // --- ADD THIS LOGIC ---
            // If an invoice URL was generated and passed, save it.
            if (!string.IsNullOrEmpty(invoiceUrl))
            {
                payment.InvoiceLink = invoiceUrl;
            }
            // --- END ADDED LOGIC ---

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<decimal> GetTotalRevenueAsync()
        {
            // Only sums payments that are marked as 'Completed'
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);
        }

       
        public async Task<int> GetPaymentCountByStatusAsync(PaymentStatus status)
        {
            return await _context.Payments
                .CountAsync(p => p.Status == status);
        }
    }
}