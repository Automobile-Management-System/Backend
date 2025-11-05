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
       var currentDate = DateTime.UtcNow;
   var startOfYear = new DateTime(currentDate.Year, 1, 1);
    
    // Total appointments (all statuses, not just completed)
            var totalAppointments = await _context.Appointments
      .CountAsync(a => a.DateTime >= startOfYear);
       
            // Total revenue (year to date)
   var totalRevenue = await _context.Payments
        .Where(p => p.Status == PaymentStatus.Completed && p.PaymentDateTime >= startOfYear)
           .SumAsync(p => p.Amount);
       
            // Average revenue per month (based on completed months)
       var monthsElapsed = currentDate.Month;
            var averageRevenuePerMonth = monthsElapsed > 0 ? totalRevenue / monthsElapsed : 0;
  
       // Growth rate (current month vs previous month)
  var currentMonthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
       var previousMonthStart = currentMonthStart.AddMonths(-1);
  
            var currentMonthRevenue = await _context.Payments
     .Where(p => p.Status == PaymentStatus.Completed && 
       p.PaymentDateTime >= currentMonthStart && 
        p.PaymentDateTime < currentMonthStart.AddMonths(1))
        .SumAsync(p => (decimal?)p.Amount) ?? 0;
 
            var previousMonthRevenue = await _context.Payments
    .Where(p => p.Status == PaymentStatus.Completed && 
    p.PaymentDateTime >= previousMonthStart && 
     p.PaymentDateTime < currentMonthStart)
     .SumAsync(p => (decimal?)p.Amount) ?? 0;
            
          var growthRate = previousMonthRevenue > 0 
         ? (double)((currentMonthRevenue - previousMonthRevenue) / previousMonthRevenue * 100) 
     : 0;

      return new AnalyticsOverviewDto
            {
  TotalRevenue = totalRevenue,
     TotalAppointments = totalAppointments,
     AverageRevenuePerMonth = averageRevenuePerMonth,
 GrowthRate = growthRate
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
           CompletedAppointments = u.EmployeeAppointments.Count(ea => ea.Appointment.Status == AppointmentStatus.Completed),
     RevenueGenerated = u.EmployeeAppointments
     .Where(ea => ea.Appointment.Status == AppointmentStatus.Completed && ea.Appointment.Payment != null)
         .Sum(ea => (decimal?)ea.Appointment.Payment.Amount) ?? 0,
          AverageRating = u.EmployeeAppointments
          .Where(ea => ea.Appointment.Review != null)
          .Average(ea => (decimal?)ea.Appointment.Review.Rating) ?? 0
         })
            .ToListAsync();

            return employees.Select(e => new EmployeePerformanceDto
            {
 EmployeeName = e.EmployeeName,
           CompletedAppointments = e.CompletedAppointments,
       RevenueGenerated = e.RevenueGenerated,
            AverageRating = e.AverageRating
       });
        }

        public async Task<RevenueStatsDto> GetRevenueStatsAsync()
        {
   var totalRevenue = await _context.Payments
  .Where(p => p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);

    // Get raw data first, then format in memory
  var revenueData = await _context.Payments
    .Where(p => p.Status == PaymentStatus.Completed)
       .GroupBy(p => new { p.PaymentDateTime.Year, p.PaymentDateTime.Month })
   .Select(g => new
           {
    Year = g.Key.Year,
       Month = g.Key.Month,
       Amount = g.Sum(p => p.Amount)
           })
  .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            var revenueByMonth = revenueData.ToDictionary(
        g => $"{g.Year}-{g.Month:D2}",
      g => g.Amount
            );

            return new RevenueStatsDto
       {
          TotalRevenue = totalRevenue,
       RevenueByMonth = revenueByMonth
            };
        }

        public async Task<RevenueTrendDto> GetRevenueTrendAsync()
        {
var currentDate = DateTime.UtcNow;
      var startDate = currentDate.AddMonths(-11); // Last 12 months
        
       // Get revenue data and format in memory
     var revenueRawData = await _context.Payments
              .Where(p => p.Status == PaymentStatus.Completed && p.PaymentDateTime >= startDate)
       .GroupBy(p => new { p.PaymentDateTime.Year, p.PaymentDateTime.Month })
        .Select(g => new
      {
             Year = g.Key.Year,
       Month = g.Key.Month,
     Revenue = g.Sum(p => p.Amount)
       })
 .OrderBy(g => g.Year).ThenBy(g => g.Month)
         .ToListAsync();

         var revenueData = revenueRawData.ToDictionary(
   g => $"{g.Year}-{g.Month:D2}",
    g => g.Revenue
      );

        // Get appointment data and format in memory
         var appointmentRawData = await _context.Appointments
        .Where(a => a.DateTime >= startDate)
       .GroupBy(a => new { a.DateTime.Year, a.DateTime.Month })
                .Select(g => new
      {
    Year = g.Key.Year,
      Month = g.Key.Month,
     Count = g.Count()
    })
           .OrderBy(g => g.Year).ThenBy(g => g.Month)
     .ToListAsync();

        var appointmentData = appointmentRawData.ToDictionary(
          g => $"{g.Year}-{g.Month:D2}",
    g => g.Count
            );

       return new RevenueTrendDto
            {
       RevenueByMonth = revenueData,
        AppointmentsByMonth = appointmentData
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
