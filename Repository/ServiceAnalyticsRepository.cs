using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Repository
{
    public class ServiceAnalyticsRepository : IServiceAnalyticsRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceAnalyticsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Service>> GetServicesAsync()
        {
            return await _context.Services.ToListAsync();
        }

        public async Task<AnalyticsOverviewDto> GetOverviewAsync()
        {
            var totalAppointments = await _context.Appointments.CountAsync();
            var completedAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed);
            var totalRevenue = await _context.Payments.Where(p => p.Status == PaymentStatus.Completed).SumAsync(p => p.Amount);
            var totalCustomers = await _context.Users.CountAsync(u => u.Role == Enums.Customer);
            var totalEmployees = await _context.Users.CountAsync(u => u.Role == Enums.Employee);

            return new AnalyticsOverviewDto
            {
                TotalAppointments = totalAppointments,
                CompletedAppointments = completedAppointments,
                TotalRevenue = totalRevenue,
                TotalCustomers = totalCustomers,
                TotalEmployees = totalEmployees
            };
        }

        public async Task<IEnumerable<ServiceCompletionDto>> GetServiceCompletionRatesAsync()
        {
            var services = await _context.Services
                .Select(s => new
                {
                    ServiceName = s.ServiceName,
                    TotalAppointments = s.AppointmentServices.Count,
                    CompletedAppointments = s.AppointmentServices.Count(aps => aps.Appointment.Status == AppointmentStatus.Completed)
                })
                .ToListAsync();

            return services.Select(s => new ServiceCompletionDto
            {
                ServiceName = s.ServiceName,
                TotalAppointments = s.TotalAppointments,
                CompletedAppointments = s.CompletedAppointments,
                CompletionRate = s.TotalAppointments > 0 ? (double)s.CompletedAppointments / s.TotalAppointments * 100 : 0
            });
        }

        public async Task<IEnumerable<EmployeePerformanceDto>> GetEmployeePerformanceAsync()
        {
            var employees = await _context.Users
                .Where(u => u.Role == Enums.Employee)
                .Select(u => new
                {
                    EmployeeName = u.FirstName + " " + u.LastName,
                    AppointmentsHandled = u.EmployeeAppointments.Count,
                    AverageRating = u.EmployeeAppointments
                        .Where(ea => ea.Appointment.Review != null)
                        .Average(ea => (decimal?)ea.Appointment.Review.Rating) ?? 0,
                    TotalHoursLogged = u.TimeLogs.Sum(tl => tl.HoursLogged)
                })
                .ToListAsync();

            return employees.Select(e => new EmployeePerformanceDto
            {
                EmployeeName = e.EmployeeName,
                AppointmentsHandled = e.AppointmentsHandled,
                AverageRating = e.AverageRating,
                TotalHoursLogged = e.TotalHoursLogged
            });
        }

        public async Task<RevenueStatsDto> GetRevenueStatsAsync()
        {
            var totalRevenue = await _context.Payments.Where(p => p.Status == PaymentStatus.Completed).SumAsync(p => p.Amount);

            var revenueByMonth = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .GroupBy(p => new { p.PaymentDateTime.Year, p.PaymentDateTime.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Amount = g.Sum(p => p.Amount)
                })
                .ToDictionaryAsync(g => g.Month, g => g.Amount);

            return new RevenueStatsDto
            {
                TotalRevenue = totalRevenue,
                RevenueByMonth = revenueByMonth
            };
        }

        public async Task<CustomerActivityDto> GetCustomerActivityAsync()
        {
            var totalCustomers = await _context.Users.CountAsync(u => u.Role == Enums.Customer);
            var activeCustomers = await _context.Users
                .Where(u => u.Role == Enums.Customer && u.Appointments.Any(a => a.DateTime >= DateTime.UtcNow.AddMonths(-1)))
                .CountAsync();
            var averageAppointmentsPerCustomer = totalCustomers > 0 ? await _context.Appointments.CountAsync() / (double)totalCustomers : 0;
            var averageRating = await _context.Reviews.AverageAsync(r => (double?)r.Rating) ?? 0;

            return new CustomerActivityDto
            {
                TotalCustomers = totalCustomers,
                ActiveCustomers = activeCustomers,
                AverageAppointmentsPerCustomer = averageAppointmentsPerCustomer,
                AverageRating = averageRating
            };
        }

        public async Task<int> SaveReportAsync(Report report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report.ReportId;
        }
    }
}
