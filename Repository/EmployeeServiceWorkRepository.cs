using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.Repository
{
    public class EmployeeServiceWorkRepository : IEmployeeServiceWorkRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeServiceWorkRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TimeLog>> GetEmployeeWorkAsync()
        {
            return await _context.TimeLogs.ToListAsync();
        }

        //added to get appointent count 
        public async Task<IEnumerable<object>> GetEmployeeAssignedAppointmentCountsAsync()
{
    return await _context.EmployeeAppointments
        .Include(ea => ea.User)
        .Include(ea => ea.Appointment)
        .Where(ea => ea.Appointment.Status == AppointmentStatus.Upcoming ||
                     ea.Appointment.Status == AppointmentStatus.InProgress)
        .GroupBy(ea => new { ea.User.UserId, ea.User.FirstName, ea.User.LastName })
        .Select(g => new
        {
            employeeId = g.Key.UserId,
            employeeName = g.Key.FirstName + " " + g.Key.LastName,
            assignedCount = g.Count()
        })
        .ToListAsync();
}

    }
}
