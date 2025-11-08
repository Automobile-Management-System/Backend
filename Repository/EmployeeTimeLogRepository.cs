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

            // Search Filter
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

            // Group all timelogs of same appointment
            var groupedQuery = query
                .GroupBy(t => new
                {
                    t.AppointmentId,
                    CustomerName = t.Appointment.User.FirstName + " " + t.Appointment.User.LastName,
                    VehicleReg = t.Appointment.CustomerVehicle.RegistrationNumber
                });

            var totalCount = await groupedQuery.CountAsync();

            var logs = await groupedQuery
                .Select(g => new EmployeeTimeLogDTO
                {
                    LogId = g.First().LogId,

                    CustomerName = g.Key.CustomerName,
                    VehicleRegNumber = g.Key.VehicleReg,

                    // earliest and latest
                    StartDateTime = g.Min(x => x.StartDateTime),
                    EndDateTime = g.Max(x => x.EndDateTime),

                    // sum hours
                    HoursLogged = g.Sum(x => x.HoursLogged),

                    CompletedServices = g.First().Appointment.Status == AppointmentStatus.Completed
                        ? g.First().Appointment.AppointmentServices
                            .Select(aps => aps.Service.ServiceName)
                            .ToList()
                        : new List<string>(),

                    CompletedModifications = g.First().Appointment.Status == AppointmentStatus.Completed
                        ? g.First().Appointment.ModificationRequests
                            .Select(m => m.Title)
                            .ToList()
                        : new List<string>()
                })
                .OrderByDescending(x => x.StartDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
