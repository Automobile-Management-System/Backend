using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IPaymentRepository
    {
        // OLD
        Task<IEnumerable<Payment>> GetAllAsync(); // You can keep or remove this

        // UPDATED - Now includes related data needed for DTOs and validation
        Task<Payment?> GetByAppointmentIdAsync(int appointmentId);
        
        Task<Payment> UpdateAsync(Payment payment);

        // NEW - Gets all payments (with Appointment details) for a specific user ID
        Task<IEnumerable<Payment>> GetPaymentsForUserAsync(int userId);
    }
}