using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        // ADD THESE TWO METHODS:
        Task<Payment?> GetByAppointmentIdAsync(int appointmentId);
        Task<Payment> UpdateAsync(Payment payment);
    }
}
