using automobile_backend.Models.DTOs; // Import DTOs
using automobile_backend.Models.Entities;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IServiceProgressRepository
    {
        // MODIFIED: This now returns the DTO directly
        Task<IEnumerable<ServiceProgressDto>> GetEmployeeServiceProgressAsync(int employeeId);

        Task<Appointment?> GetAppointmentWithDetailsAsync(int appointmentId);
        Task<TimeLog?> GetActiveTimerAsync(int appointmentId, int userId);
        Task<IEnumerable<TimeLog>> GetTimeLogsByAppointmentAsync(int appointmentId);
        Task<TimeLog> CreateTimeLogAsync(TimeLog timeLog);
        Task<TimeLog> UpdateTimeLogAsync(TimeLog timeLog);
        Task<Appointment> UpdateAppointmentAsync(Appointment appointment);
        Task<decimal> GetTotalLoggedTimeAsync(int appointmentId);
    }
}