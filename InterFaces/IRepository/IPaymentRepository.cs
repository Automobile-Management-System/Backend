using automobile_backend.Models.Entities;
using System.Linq;
using System.Threading.Tasks; // Added

namespace automobile_backend.InterFaces.IRepository
{
    public interface IPaymentRepository
    {
        /// <summary>
        /// Gets a queryable list of appointments for a user, including related
        /// payment and service details for invoice generation.
        /// </summary>
        /// <param name="userId">The ID of the user (customer).</param>
        /// <returns>An IQueryable<Appointment> with included entities.</returns>
        IQueryable<Appointment> GetCustomerAppointmentsWithDetails(int userId);
        
        /// <summary>
        /// Gets a single appointment by its ID, including details needed for payment.
        /// </summary>
        /// <param name="appointmentId">The ID of the appointment.</param>
        /// <returns>The Appointment entity or null if not found.</returns>
        Task<Appointment> GetAppointmentForPaymentAsync(int appointmentId);

        /// <summary>
        /// Creates a new payment record in the database.
        /// </summary>
        /// <param name="payment">The payment entity to add.</param>
        Task CreatePaymentRecordAsync(Payment payment);
    }
}