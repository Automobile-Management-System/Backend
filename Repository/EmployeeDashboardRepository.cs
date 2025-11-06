using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace automobile_backend.Repository
{
    public class EmployeeDashboardRepository : IEmployeeDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeDashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetUpcomingAppointmentCountAsync(int employeeId)
        {
            var today = DateTime.Today;

            var count = await _context.EmployeeAppointments
                .Include(ea => ea.Appointment)
                .Where(ea =>
                    ea.UserId == employeeId &&
                    ea.Appointment.Status == AppointmentStatus.Upcoming)
                .CountAsync();

            return count;
        }

        public async Task<int> GetInProgressAppointmentCountAsync(int employeeId)
        {
            var count = await _context.EmployeeAppointments
                .Include(ea => ea.Appointment)
                .Where(ea =>
                    ea.UserId == employeeId &&
                    ea.Appointment.Status == AppointmentStatus.InProgress)
                .CountAsync();

            return count;
        }

        public async Task<List<object>> GetRecentServicesAsync(int employeeId)
        {
           

            var services = await _context.EmployeeAppointments
                .Include(ea => ea.Appointment)
                    .ThenInclude(a => a.AppointmentServices)
                        .ThenInclude(aps => aps.Service)
                .Where(ea =>
                    ea.UserId == employeeId)
                .OrderByDescending(ea => ea.Appointment.DateTime)
                .Take(5)
                .SelectMany(ea => ea.Appointment.AppointmentServices.Select(aps => new
                {
                    ServiceName = aps.Service.ServiceName,
                    AppointmentStatus = ea.Appointment.Status.ToString(),
                    VehicleId = ea.Appointment.VehicleId,
                    Date = ea.Appointment.DateTime
                }))
                .ToListAsync<object>();

            return services;
        }


        public async Task<List<object>> GetRecentModificationsAsync(int employeeId)
        {
            

            var modifications = await _context.EmployeeAppointments
                .Include(ea => ea.Appointment)
                    .ThenInclude(a => a.ModificationRequests)
                .Where(ea =>
                    ea.UserId == employeeId &&
                    ea.Appointment.ModificationRequests.Any())
                .OrderByDescending(ea => ea.Appointment.DateTime)
                .Take(5)
                .SelectMany(ea => ea.Appointment.ModificationRequests.Select(m => new
                {
                    ModificationTitle = m.Title,
                    AppointmentStatus = ea.Appointment.Status.ToString(),
                    VehicleId = ea.Appointment.VehicleId,
                    Date = ea.Appointment.DateTime
                }))
                .ToListAsync<object>();

            return modifications;
        }

        public async Task<int> GetCompletedServiceCountAsync(int employeeId)
        {
            var count = await _context.EmployeeAppointments
                .Include(ea => ea.Appointment)
                    .ThenInclude(a => a.AppointmentServices)
                .Where(ea =>
                    ea.UserId == employeeId &&
                    ea.Appointment.Status == AppointmentStatus.Completed &&
                    ea.Appointment.AppointmentServices.Any())
                .CountAsync();

            return count;
        }

        public async Task<int> GetCompletedModificationCountAsync(int employeeId)
        {
            var count = await _context.EmployeeAppointments
                .Include(ea => ea.Appointment)
                    .ThenInclude(a => a.ModificationRequests)
                .Where(ea =>
                    ea.UserId == employeeId &&
                    ea.Appointment.Status == AppointmentStatus.Completed &&
                    ea.Appointment.ModificationRequests.Any())
                .CountAsync();

            return count;
        }




    }
}
