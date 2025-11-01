using automobile_backend.Models.Entities;
using automobile_backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace automobile_backend.Repositories
{
    public class CustomerVehicleRepository : ICustomerVehicleRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerVehicleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerVehicle>> GetVehiclesByUserIdAsync(int userId)
        {
            return await _context.CustomerVehicles
                .Where(v => v.UserId == userId)
                .ToListAsync();
        }

        public async Task<CustomerVehicle?> GetVehicleByIdAsync(int vehicleId, int userId)
        {
            return await _context.CustomerVehicles
                .FirstOrDefaultAsync(v => v.VehicleId == vehicleId && v.UserId == userId);
        }

        public async Task AddVehicleAsync(CustomerVehicle vehicle)
        {
            await _context.CustomerVehicles.AddAsync(vehicle);
        }

        public async Task UpdateVehicleAsync(CustomerVehicle vehicle)
        {
            _context.CustomerVehicles.Update(vehicle);
        }

        public async Task DeleteVehicleAsync(CustomerVehicle vehicle)
        {
            _context.CustomerVehicles.Remove(vehicle);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
