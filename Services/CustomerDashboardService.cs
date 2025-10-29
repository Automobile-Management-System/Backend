using automobile_backend.Interfaces.IRepositories;
using automobile_backend.Interfaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class CustomerDashboardService : ICustomerDashboardService
    {
        private readonly ICustomerDashboardRepository _repository;

        public CustomerDashboardService(ICustomerDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task AddVehicleAsync(int userId, CustomerVehicleDto dto)
        {
            var vehicle = new CustomerVehicle
            {
                RegistrationNumber = dto.RegistrationNumber,
                FuelType = Enum.Parse<FuelType>(dto.FuelType, true),
                ChassisNumber = dto.ChassisNumber,
                Brand = dto.Brand,
                Model = dto.Model,
                Year = dto.Year,
                UserId = userId
            };

            await _repository.AddVehicleAsync(vehicle);
        }

        public async Task<List<CustomerVehicleDto>> GetVehiclesByUserIdAsync(int userId)
        {
            var vehicles = await _repository.GetVehiclesByUserIdAsync(userId);

            return vehicles.Select(v => new CustomerVehicleDto
            {
                RegistrationNumber = v.RegistrationNumber,
                FuelType = v.FuelType.ToString(),
                ChassisNumber = v.ChassisNumber,
                Brand = v.Brand,
                Model = v.Model,
                Year = v.Year
            }).ToList();
        }

        public async Task<int> GetUpcomingAppointmentsCountAsync(int userId)
            => await _repository.GetUpcomingAppointmentsCountAsync(userId);

        public async Task<int> GetInProgressAppointmentsCountAsync(int userId)
            => await _repository.GetInProgressAppointmentsCountAsync(userId);

        public async Task<int> GetCompletedAppointmentsCountAsync(int userId)
            => await _repository.GetCompletedAppointmentsCountAsync(userId);

        public async Task<decimal> GetPendingPaymentsTotalAsync(int userId)
            => await _repository.GetPendingPaymentsTotalAsync(userId);

        public Task<IEnumerable<Appointment>> GetLatestServicesAsync(int userId)
           => _repository.GetLatestServicesAsync(userId);

        public Task<IEnumerable<Appointment>> GetLatestModificationsAsync(int userId)
            => _repository.GetLatestModificationsAsync(userId);
    }
}
