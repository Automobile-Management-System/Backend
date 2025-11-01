using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using automobile_backend.InterFaces.IServices;
using System;

public class VehicleService : IVehicleService
{
    private readonly ApplicationDbContext _context;

    public VehicleService(ApplicationDbContext context)
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
                FuelType = v.FuelType.ToString()
            })
            .ToListAsync();
    }
}
