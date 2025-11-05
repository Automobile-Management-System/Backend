using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Repository
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardRepository(ApplicationDbContext context)
        {
            _context = context;
     }

        public async Task<AdminDashboardOverviewDto> GetDashboardOverviewAsync()
        {
          var totalRevenue = await _context.Payments
       .Where(p => p.Status == PaymentStatus.Completed)
         .SumAsync(p => (decimal?)p.Amount) ?? 0;

     var totalUsers = await _context.Users.CountAsync();

            var totalCustomers = await _context.Users
          .CountAsync(u => u.Role == Enums.Customer);

            var totalAppointments = await _context.Appointments.CountAsync();

         return new AdminDashboardOverviewDto
       {
 TotalRevenue = totalRevenue,
      TotalUsers = totalUsers,
 TotalCustomers = totalCustomers,
    TotalAppointments = totalAppointments
          };
        }

        public async Task<WeeklyRevenueDto> GetWeeklyRevenueAsync()
        {
     // Get the start of the current week (Monday)
            var today = DateTime.Now.Date;
         var dayOfWeek = (int)today.DayOfWeek;
var startOfWeek = today.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1)); // Monday
            var endOfWeek = startOfWeek.AddDays(6); // Sunday

    // Get revenue data for the week
            var revenueData = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && 
              p.PaymentDateTime >= startOfWeek && 
         p.PaymentDateTime <= endOfWeek.AddDays(1))
      .GroupBy(p => p.PaymentDateTime.Date)
         .Select(g => new
       {
     Date = g.Key,
   Revenue = g.Sum(p => p.Amount)
             })
                .ToListAsync();

            var result = new WeeklyRevenueDto();
 
            // Generate data for all 7 days of the week
  var dayNames = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            
            for (int i = 0; i < 7; i++)
        {
    var currentDate = startOfWeek.AddDays(i);
  var dayRevenue = revenueData.FirstOrDefault(d => d.Date == currentDate)?.Revenue ?? 0;
       
result.Days.Add(dayNames[i]);
       result.RevenueList.Add(dayRevenue);
            }

            return result;
        }

public async Task<WeeklyAppointmentsDto> GetWeeklyAppointmentsAsync()
        {
// Get the start of the current week (Monday)
            var today = DateTime.Now.Date;
     var dayOfWeek = (int)today.DayOfWeek;
    var startOfWeek = today.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1)); // Monday
      var endOfWeek = startOfWeek.AddDays(6); // Sunday

            // Get appointment data for the week
      var appointmentData = await _context.Appointments
             .Where(a => a.DateTime >= startOfWeek && 
            a.DateTime <= endOfWeek.AddDays(1))
                .GroupBy(a => a.DateTime.Date)
        .Select(g => new
       {
     Date = g.Key,
          Count = g.Count()
                })
    .ToListAsync();

      var result = new WeeklyAppointmentsDto();
            
      // Generate data for all 7 days of the week
  var dayNames = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            
          for (int i = 0; i < 7; i++)
       {
          var currentDate = startOfWeek.AddDays(i);
      var dayAppointments = appointmentData.FirstOrDefault(d => d.Date == currentDate)?.Count ?? 0;
                
     result.Days.Add(dayNames[i]);
        result.Appointments.Add(dayAppointments);
            }

      return result;
    }

        public async Task<IEnumerable<RecentUserDto>> GetRecentUsersAsync(int count = 10)
    {
            // Get most recent users by UserId (assuming higher IDs = more recent)
   var users = await _context.Users
                .OrderByDescending(u => u.UserId)
                .Take(count)
       .Select(u => new RecentUserDto
          {
         UserId = u.UserId,
          FullName = u.FirstName + " " + u.LastName,
      Email = u.Email,
      Role = u.Role.ToString(),
       ProfilePicture = u.ProfilePicture,
     RegisteredDate = DateTime.Now.AddDays(-u.UserId % 30) // Mock date based on UserId
        })
           .ToListAsync();

            return users;
        }
    }
}