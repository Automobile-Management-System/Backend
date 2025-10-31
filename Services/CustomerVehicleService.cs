using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using automobile_backend.Repositories.Interfaces;
using automobile_backend.Services.Interfaces;

namespace automobile_backend.Services
{
    public class CustomerVehicleService : ICustomerVehicleService
    {
        private readonly ICustomerVehicleRepository _repository;

        public CustomerVehicleService(ICustomerVehicleRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ViewVehicleDto>> GetUserVehiclesAsync(int userId)
        {
            var vehicles = await _repository.GetVehiclesByUserIdAsync(userId);
            return vehicles.Select(v => new ViewVehicleDto
            {
                VehicleId = v.VehicleId,
                RegistrationNumber = v.RegistrationNumber,
                FuelType = v.FuelType.ToString(),
                ChassisNumber = v.ChassisNumber,
                Brand = v.Brand,
                Model = v.Model,
                Year = v.Year
            });
        }

        public async Task<ViewVehicleDto?> GetVehicleByIdAsync(int vehicleId, int userId)
        {
            var v = await _repository.GetVehicleByIdAsync(vehicleId, userId);
            if (v == null) return null;

            return new ViewVehicleDto
            {
                VehicleId = v.VehicleId,
                RegistrationNumber = v.RegistrationNumber,
                FuelType = v.FuelType.ToString(),
                ChassisNumber = v.ChassisNumber,
                Brand = v.Brand,
                Model = v.Model,
                Year = v.Year
            };
        }

        public async Task<bool> AddVehicleAsync(int userId, CreateVehicleDto dto)
        {
            var vehicle = new CustomerVehicle
            {
                RegistrationNumber = dto.RegistrationNumber,
                FuelType = Enum.Parse<FuelType>(dto.FuelType),
                ChassisNumber = dto.ChassisNumber,
                Brand = dto.Brand,
                Model = dto.Model,
                Year = dto.Year,
                UserId = userId
            };

            await _repository.AddVehicleAsync(vehicle);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateVehicleAsync(int vehicleId, int userId, UpdateVehicleDto dto)
        {
            var vehicle = await _repository.GetVehicleByIdAsync(vehicleId, userId);
            if (vehicle == null) return false;

            vehicle.RegistrationNumber = dto.RegistrationNumber;
            vehicle.FuelType = Enum.Parse<FuelType>(dto.FuelType);
            vehicle.ChassisNumber = dto.ChassisNumber;
            vehicle.Brand = dto.Brand;
            vehicle.Model = dto.Model;
            vehicle.Year = dto.Year;

            await _repository.UpdateVehicleAsync(vehicle);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteVehicleAsync(int vehicleId, int userId)
        {
            var vehicle = await _repository.GetVehicleByIdAsync(vehicleId, userId);
            if (vehicle == null) return false;

            await _repository.DeleteVehicleAsync(vehicle);
            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
