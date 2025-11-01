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

        public async Task<List<TimeLog>> GetEmployeeTimeLogsAsync(int userId)
        {
            return await _context.TimeLogs
                .Include(t => t.Appointment)
                    .ThenInclude(a => a.User)
                .Include(t => t.Appointment)
                    .ThenInclude(a => a.AppointmentServices)
                        .ThenInclude(aps => aps.Service)
                .Include(t => t.Appointment)
                    .ThenInclude(a => a.ModificationRequests)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.StartDateTime)
                .ToListAsync();
        }
    }
}
