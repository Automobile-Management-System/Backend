using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.DTOs; // Import DTOs
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

        // --- NEW EFFICIENT METHOD ---
        // This replaces the old GetEmployeeAppointmentsAsync
        public async Task<IEnumerable<ServiceProgressDto>> GetEmployeeServiceProgressAsync(int employeeId)
        {
            return await _context.Appointments
                .Where(a => a.EmployeeAppointments.Any(ea => ea.UserId == employeeId))
                .Where(a => a.Status == AppointmentStatus.Upcoming ||
                            a.Status == AppointmentStatus.InProgress ||
                            a.Status == AppointmentStatus.Completed)
                .Select(a => new ServiceProgressDto
                {
                    AppointmentId = a.AppointmentId,
                    CustomerName = a.User.FirstName + " " + a.User.LastName,
                    CustomerId = a.UserId, // Added this
                    CustomerVehicleName = a.CustomerVehicle.Brand + " " + a.CustomerVehicle.Model,
                    Status = a.Status,
                    ServiceType = a.Type,
                    AppointmentDateTime = a.DateTime,

                    // Sub-query for active timer (scoped to this employee)
                    IsTimerActive = a.TimeLogs.Any(tl => tl.UserId == employeeId && tl.IsActive),
                    CurrentTimerStartTime = a.TimeLogs
                                            .Where(tl => tl.UserId == employeeId && tl.IsActive)
                                            .Select(tl => (DateTime?)tl.StartDateTime) // Cast to nullable
                                            .FirstOrDefault(),

                    // Sub-query for total time (all employees on this job)
                    TotalTimeLogged = a.TimeLogs
                                      .Where(tl => !tl.IsActive)
                                      .Sum(tl => tl.HoursLogged),

                    // Sub-queries for details
                    ServiceNames = (a.Type == Models.Entities.Type.Service)
                                    ? a.AppointmentServices.Select(asv => asv.Service.ServiceName).ToList()
                                    : new List<string>(),
                    ModificationTitle = (a.Type == Models.Entities.Type.Modifications)
                                    ? a.ModificationRequests.Select(m => m.Title).FirstOrDefault()
                                    : null,
                    ModificationDescription = (a.Type == Models.Entities.Type.Modifications)
                                    ? a.ModificationRequests.Select(m => m.Description).FirstOrDefault()
                                    : null

                    // We deliberately DO NOT include the TimeLogs list
                })
                .OrderByDescending(dto => dto.AppointmentDateTime) // Sort in the DB
                .ToListAsync();
        }
        // --- END NEW METHOD ---

        public async Task<Appointment?> GetAppointmentWithDetailsAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.CustomerVehicle)
                .Include(a => a.TimeLogs.OrderBy(tl => tl.StartDateTime))
                    .ThenInclude(tl => tl.User)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(asv => asv.Service)
                .Include(a => a.ModificationRequests)
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