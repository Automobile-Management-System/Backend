using automobile_backend.InterFaces.IRepository;
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

        public async Task<(List<TimeLog> logs, int totalCount)> GetEmployeeTimeLogsAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search,
            DateTime? startDate,
            DateTime? endDate)
        {
            var query = _context.TimeLogs
                .Include(t => t.Appointment)
                    .ThenInclude(a => a.User)
                .Include(t => t.Appointment)
                    .ThenInclude(a => a.AppointmentServices)
                        .ThenInclude(aps => aps.Service)
                .Include(t => t.Appointment)
                    .ThenInclude(a => a.ModificationRequests)
                .Where(t => t.UserId == userId)
                .AsQueryable();

            // 🔍 Apply search filter (customer name)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(t =>
                    (t.Appointment.User.FirstName + " " + t.Appointment.User.LastName).ToLower().Contains(lowerSearch));
            }

            // 📅 Apply date range filter
            if (startDate.HasValue)
                query = query.Where(t => t.StartDateTime.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(t => t.EndDateTime.HasValue && t.EndDateTime.Value.Date <= endDate.Value.Date);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(t => t.StartDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (logs, totalCount);
        }
    }
}
