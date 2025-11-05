using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // Shows ALL employees with their total ongoing/upcoming assignment counts
        public async Task<IEnumerable<object>> GetEmployeeAssignedAppointmentCountsAsync()
        {
            // Get ALL users with role "Employee"
            var allEmployees = await _context.Users
.Where(u => u.Role == Enums.Employee)

                .ToListAsync();

            // Get appointment counts grouped by employee
            var appointmentCounts = await _context.EmployeeAppointments
                .Include(ea => ea.Appointment)
                .Where(ea => ea.Appointment.Status == AppointmentStatus.Upcoming ||
                             ea.Appointment.Status == AppointmentStatus.InProgress)
                .GroupBy(ea => ea.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Combine: all employees with their counts (0 if no appointments)
            return allEmployees.Select(emp => new
            {
                employeeId = emp.UserId,
                employeeName = $"{emp.FirstName} {emp.LastName}",
                assignedCount = appointmentCounts
                    .FirstOrDefault(ac => ac.UserId == emp.UserId)?.Count ?? 0
            });
        }

        // ADD THIS METHOD: Shows ALL employees with their assignment counts for a specific date
        public async Task<IEnumerable<object>> GetAllEmployeesWithDailyAssignmentCountAsync(DateTime date)
        {
            var targetDate = date.Date;

            // Get ALL users with role "Employee"
            var allEmployees = await _context.Users
.Where(u => u.Role == Enums.Employee)
                .ToListAsync();

            // Get appointment counts for the specific date grouped by employee
            var dailyAppointmentCounts = await _context.EmployeeAppointments
                .Include(ea => ea.Appointment)
                .Where(ea => ea.Appointment.DateTime.Date == targetDate)
                .GroupBy(ea => ea.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // Combine: all employees with their daily counts (0 if no appointments on that date)
            return allEmployees.Select(emp => new
            {
                employeeId = emp.UserId,
                employeeName = $"{emp.FirstName} {emp.LastName}",
                assignedCount = dailyAppointmentCounts
                    .FirstOrDefault(ac => ac.UserId == emp.UserId)?.Count ?? 0
            });
        }
    }
}