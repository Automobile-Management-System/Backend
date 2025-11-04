
using automobile_backend.InterFaces.IRepositories;
using automobile_backend.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly ApplicationDbContext _context;

        public VehicleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VehicleDto>> GetVehiclesByUserIdAsync(int userId)
        {
            return await _context.CustomerVehicles
                .Where(v => v.UserId == userId)
                .Select(v => new VehicleDto
                {
                    VehicleId = v.VehicleId,
                    RegistrationNumber = v.RegistrationNumber,
                    Brand = v.Brand,
                    Model = v.Model,
                    Year = v.Year,
                    FuelType = v.FuelType.ToString(),
                    UserId = v.UserId
                })
                .ToListAsync();
        }
    }
}
