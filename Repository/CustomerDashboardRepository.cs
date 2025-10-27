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
    }
}
