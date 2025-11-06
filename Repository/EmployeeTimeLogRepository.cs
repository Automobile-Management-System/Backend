using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.DTO;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace automobile_backend.Repository
{
    public class EmployeeTimeLogRepository : IEmployeeTimeLogRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeTimeLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResponse<EmployeeTimeLogDTO>> GetEmployeeLogsAsync(
            int employeeId,
            int pageNumber,
            int pageSize,
            string? search)
        {
            var query = _context.TimeLogs
                .Where(t => t.UserId == employeeId && t.EndDateTime != null)
                .Include(t => t.Appointment)
                    .ThenInclude(a => a.User)
                .Include(t => t.Appointment)
                    .ThenInclude(a => a.CustomerVehicle)
                .Include(t => t.Appointment.AppointmentServices)
                    .ThenInclude(aps => aps.Service)
                .Include(t => t.Appointment.ModificationRequests)
                .AsQueryable();

            // ✅ Search filter (customer name, vehicle reg, service name, modification title)
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.ToLower();
                query = query.Where(t =>
                    t.Appointment.User.FirstName.ToLower().Contains(s) ||
                    t.Appointment.User.LastName.ToLower().Contains(s) ||
                    t.Appointment.CustomerVehicle.RegistrationNumber.ToLower().Contains(s) ||
                    t.Appointment.AppointmentServices.Any(aps => aps.Service.ServiceName.ToLower().Contains(s)) ||
                    t.Appointment.ModificationRequests.Any(m => m.Title.ToLower().Contains(s))
                );
            }

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new EmployeeTimeLogDTO
                {
                    LogId = t.LogId,
                    CustomerName = t.Appointment.User.FirstName + " " + t.Appointment.User.LastName,
                    VehicleRegNumber = t.Appointment.CustomerVehicle.RegistrationNumber,
                    StartDateTime = t.StartDateTime,
                    EndDateTime = t.EndDateTime,
                    HoursLogged = t.HoursLogged,
                    CompletedServices = t.Appointment.Status == AppointmentStatus.Completed
                    ? t.Appointment.AppointmentServices
                        .Select(aps => aps.Service.ServiceName)
                        .ToList()
                        : null,
                    CompletedModifications = t.Appointment.Status == AppointmentStatus.Completed
    ? t.Appointment.ModificationRequests
        .Select(m => m.Title)
        .ToList()
    : null
                })
                .ToListAsync();

            return new PaginatedResponse<EmployeeTimeLogDTO>
            {
                Data = logs,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
