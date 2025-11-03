using automobile_backend.Interfaces.IRepositories;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.Repositories
{
    public class CustomerDashboardRepository : ICustomerDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerDashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddVehicleAsync(CustomerVehicle vehicle)
        {
            _context.CustomerVehicles.Add(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CustomerVehicle>> GetVehiclesByUserIdAsync(int userId)
        {
            return await _context.CustomerVehicles
                                 .Where(v => v.UserId == userId)
                                 .Include(v => v.User)
                                 .ToListAsync();
        }

        public async Task<int> GetUpcomingAppointmentsCountAsync(int userId)
        {
            return await _context.Appointments
                .Where(a => a.UserId == userId && a.Status == AppointmentStatus.Upcoming)
                .CountAsync();
        }

        public async Task<int> GetInProgressAppointmentsCountAsync(int userId)
        {
            return await _context.Appointments
                .Where(a => a.UserId == userId && a.Status == AppointmentStatus.InProgress)
                .CountAsync();
        }

        public async Task<int> GetCompletedAppointmentsCountAsync(int userId)
        {
            return await _context.Appointments
                .Where(a => a.UserId == userId && a.Status == AppointmentStatus.Completed)
                .CountAsync();
        }

        public async Task<decimal> GetPendingPaymentsTotalAsync(int userId)
        {
            return await _context.Payments
                .Include(p => p.Appointment)
                .Where(p => p.Appointment.UserId == userId && p.Status == PaymentStatus.Pending)
                .SumAsync(p => (decimal?)p.Amount ?? 0);
        }

        public async Task<List<object>> GetLatestServicesAsync(int userId)
        {
            return await _context.Appointments
                .Include(a => a.AppointmentServices)
                    .ThenInclude(asv => asv.Service)
                .Where(a => a.UserId == userId && a.Type == Models.Entities.Type.Service)
                .OrderByDescending(a => a.DateTime)
                .Take(3)
                .Select(a => new
                {
                    Date = a.DateTime,
                    Services = a.AppointmentServices
                        .Select(asv => asv.Service.ServiceName)
                        .ToList()
                })
                .ToListAsync<object>();
        }

        public async Task<List<object>> GetLatestModificationsAsync(int userId)
        {
            return await _context.Appointments
                .Include(a => a.ModificationRequests)
                .Where(a => a.UserId == userId && a.Type == Models.Entities.Type.Modifications)
                .OrderByDescending(a => a.DateTime)
                .Take(3)
                .Select(a => new
                {
                    Date = a.DateTime,
                    Modifications = a.ModificationRequests
                        .Select(m => m.Title)
                        .ToList()
                })
                .ToListAsync<object>();
        }

    }
}
