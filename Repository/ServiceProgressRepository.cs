using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace automobile_backend.Repository
{
    public class ServiceProgressRepository : IServiceProgressRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceProgressRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Appointment>> GetEmployeeAppointmentsAsync(int employeeId)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.CustomerVehicle)
                .Include(a => a.TimeLogs)
                .Include(a => a.EmployeeAppointments)
                .Include(a => a.AppointmentServices) // Added
                    .ThenInclude(asv => asv.Service)  // Added
                .Include(a => a.ModificationRequests) // Added
                .Where(a => a.EmployeeAppointments.Any(ea => ea.UserId == employeeId))
                .Where(a => a.Status == AppointmentStatus.Upcoming ||
                            a.Status == AppointmentStatus.InProgress ||
                            a.Status == AppointmentStatus.Completed)
                .OrderBy(a => a.DateTime)
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentWithDetailsAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.CustomerVehicle)
                .Include(a => a.TimeLogs.OrderBy(tl => tl.StartDateTime))
                    .ThenInclude(tl => tl.User)
                .Include(a => a.AppointmentServices) // Added
                    .ThenInclude(asv => asv.Service)  // Added
                .Include(a => a.ModificationRequests) // Added
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        }

        public async Task<TimeLog?> GetActiveTimerAsync(int appointmentId, int userId)
        {
            return await _context.TimeLogs
                .FirstOrDefaultAsync(tl => tl.AppointmentId == appointmentId
                                         && tl.UserId == userId
                                         && tl.IsActive);
        }

        public async Task<IEnumerable<TimeLog>> GetTimeLogsByAppointmentAsync(int appointmentId)
        {
            return await _context.TimeLogs
                .Include(tl => tl.User)
                .Where(tl => tl.AppointmentId == appointmentId)
                .OrderBy(tl => tl.StartDateTime)
                .ToListAsync();
        }

        public async Task<TimeLog> CreateTimeLogAsync(TimeLog timeLog)
        {
            _context.TimeLogs.Add(timeLog);
            await _context.SaveChangesAsync();
            return timeLog;
        }

        public async Task<TimeLog> UpdateTimeLogAsync(TimeLog timeLog)
        {
            _context.TimeLogs.Update(timeLog);
            await _context.SaveChangesAsync();
            return timeLog;
        }

        public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<decimal> GetTotalLoggedTimeAsync(int appointmentId)
        {
            return await _context.TimeLogs
                .Where(tl => tl.AppointmentId == appointmentId && !tl.IsActive)
                .SumAsync(tl => tl.HoursLogged);
        }
    }
}