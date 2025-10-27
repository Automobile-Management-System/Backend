using automobile_backend.Models.Entities;
using System.Linq;

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
    }
}