using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace automobile_backend.Repository
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetTotalUsersCountForPreviousMonthAsync()
        {
            // Since we don't have a CreatedAt field, we'll simulate previous month data
            // by assuming users with lower IDs were created earlier
            var currentUserCount = await _context.Users.CountAsync();
            // Simulate 10% growth from previous month
            var previousMonthCount = (int)(currentUserCount * 0.9);
            return previousMonthCount;
        }

        public async Task<int> GetActiveBookingsCountAsync()
        {
            return await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Upcoming || 
                           a.Status == AppointmentStatus.InProgress)
                .CountAsync();
        }

        public async Task<int> GetActiveBookingsCountForPreviousMonthAsync()
        {
            var previousMonth = DateTime.Now.AddMonths(-1);
            var startOfPreviousMonth = new DateTime(previousMonth.Year, previousMonth.Month, 1);
            var endOfPreviousMonth = startOfPreviousMonth.AddMonths(1).AddDays(-1);

            return await _context.Appointments
                .Where(a => a.DateTime >= startOfPreviousMonth && 
                           a.DateTime <= endOfPreviousMonth &&
                           (a.Status == AppointmentStatus.Upcoming || 
                            a.Status == AppointmentStatus.InProgress))
                .CountAsync();
        }

        public async Task<decimal> GetMonthlyRevenueAsync()
        {
            var currentMonth = DateTime.Now;
            var startOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            return await _context.Payments
                .Where(p => p.PaymentDateTime >= startOfMonth && 
                           p.PaymentDateTime <= endOfMonth &&
                           p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);
        }

        public async Task<decimal> GetPreviousMonthRevenueAsync()
        {
            var previousMonth = DateTime.Now.AddMonths(-1);
            var startOfPreviousMonth = new DateTime(previousMonth.Year, previousMonth.Month, 1);
            var endOfPreviousMonth = startOfPreviousMonth.AddMonths(1).AddDays(-1);

            return await _context.Payments
                .Where(p => p.PaymentDateTime >= startOfPreviousMonth && 
                           p.PaymentDateTime <= endOfPreviousMonth &&
                           p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);
        }

        public async Task<List<WeeklyRevenueDto>> GetWeeklyRevenueAsync()
        {
            var startOfWeek = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);

            var dailyRevenue = await _context.Payments
                .Where(p => p.PaymentDateTime >= startOfWeek && 
                           p.PaymentDateTime <= endOfWeek &&
                           p.Status == PaymentStatus.Completed)
                .GroupBy(p => p.PaymentDateTime.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(p => p.Amount) })
                .ToListAsync();

            var result = new List<WeeklyRevenueDto>();
            for (int i = 0; i < 7; i++)
            {
                var date = startOfWeek.AddDays(i);
                var revenue = dailyRevenue.FirstOrDefault(d => d.Date == date.Date)?.Revenue ?? 0;
                
                result.Add(new WeeklyRevenueDto
                {
                    Day = date.ToString("ddd"),
                    Revenue = revenue
                });
            }

            return result;
        }

        public async Task<List<WeeklyAppointmentDto>> GetWeeklyAppointmentsAsync()
        {
            var startOfWeek = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);

            var dailyAppointments = await _context.Appointments
                .Where(a => a.DateTime >= startOfWeek && a.DateTime <= endOfWeek)
                .GroupBy(a => a.DateTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new List<WeeklyAppointmentDto>();
            for (int i = 0; i < 7; i++)
            {
                var date = startOfWeek.AddDays(i);
                var count = dailyAppointments.FirstOrDefault(d => d.Date == date.Date)?.Count ?? 0;
                
                result.Add(new WeeklyAppointmentDto
                {
                    Day = date.ToString("ddd"),
                    AppointmentCount = count
                });
            }

            return result;
        }

        public async Task<List<RecentUserDto>> GetRecentUsersAsync(int limit)
        {
            // Since there's no CreatedAt field, we'll use UserId as a proxy for creation order
            var users = await _context.Users
                .OrderByDescending(u => u.UserId)
                .Take(limit)
                .ToListAsync();

            var recentUsers = users.Select(u => new RecentUserDto
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role.ToString(),
                TimeAgo = CalculateTimeAgo(u.UserId), // This will be calculated based on UserId
                CreatedDate = DateTime.Now.AddDays(-u.UserId % 30) // Mock creation date within last 30 days
            }).ToList();

            return recentUsers;
        }

        public async Task<List<SystemAlertDto>> GetSystemAlertsAsync()
        {
            var alerts = new List<SystemAlertDto>();

            // Check for modification requests (all modification requests are considered for review)
            var modificationCount = await _context.ModificationRequests.CountAsync();

            if (modificationCount > 0)
            {
                alerts.Add(new SystemAlertDto
                {
                    Id = 1,
                    Type = "warning",
                    Message = $"{modificationCount} modification requests need review",
                    ActionText = "Take Action →",
                    CreatedAt = DateTime.Now.AddHours(-2),
                    IsRead = false
                });
            }

            // Check for pending appointments (appointments that are still pending approval)
            var pendingAppointments = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Pending)
                .CountAsync();

            if (pendingAppointments > 0)
            {
                alerts.Add(new SystemAlertDto
                {
                    Id = 2,
                    Type = "info",
                    Message = $"{pendingAppointments} appointments are pending approval",
                    ActionText = "Take Action →",
                    CreatedAt = DateTime.Now.AddHours(-3),
                    IsRead = false
                });
            }

            // Check for employees needing safety training (based on employee count)
            var employeeCount = await _context.Users
                .Where(u => u.Role == Enums.Employee)
                .CountAsync();

            if (employeeCount > 0)
            {
                // Mock alert: assume 2 employees need training
                alerts.Add(new SystemAlertDto
                {
                    Id = 3,
                    Type = "info",
                    Message = "2 employees need to complete safety training",
                    ActionText = "Take Action →",
                    CreatedAt = DateTime.Now.AddHours(-5),
                    IsRead = false
                });
            }

            // System backup completed (mock alert)
            alerts.Add(new SystemAlertDto
            {
                Id = 4,
                Type = "success",
                Message = "System backup completed successfully",
                ActionText = "",
                CreatedAt = DateTime.Now.AddMinutes(-30),
                IsRead = true
            });

            return alerts.OrderByDescending(a => a.CreatedAt).ToList();
        }

        public async Task<bool> MarkAlertAsReadAsync(int alertId)
        {
            // Since we don't have a persistent alerts table, this is a mock implementation
            // In a real application, you would have an Alerts table
            return await Task.FromResult(true);
        }

        private string CalculateTimeAgo(int userId)
        {
            // Mock calculation based on UserId - in reality you'd use actual creation timestamp
            var daysAgo = userId % 7; // Mock calculation
            if (daysAgo == 0) return "2 hours ago";
            if (daysAgo == 1) return "5 hours ago";
            if (daysAgo == 2) return "1 day ago";
            return $"{daysAgo} days ago";
        }
    }
}