using automobile_backend.InterFaces.IServices;
using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class CustomerModificationRequestService : ICustomerModificationRequestService
    {
        private readonly ICustomerModificationRequestRepository _requestRepository;
        private readonly ApplicationDbContext _context;

        public CustomerModificationRequestService(
            ICustomerModificationRequestRepository requestRepository,
            ApplicationDbContext context)
        {
            _requestRepository = requestRepository;
            _context = context;
        }

        public async Task<IEnumerable<CustomerModificationRequestDto>> GetAllModificationRequestsAsync()
        {
            var requests = await _requestRepository.GetAllAsync();

            // Sort by CreatedDate descending (newest first)
            return requests
                .Select(MapToDto)
                .OrderByDescending(r => r.RequestDate);
        }

        public async Task<IEnumerable<CustomerModificationRequestDto>> GetByUserIdAsync(int userId)
        {
            var requests = await _requestRepository.GetByUserIdAsync(userId);

            // Sort by CreatedDate descending (newest first)
            return requests
                .Select(MapToDto)
                .OrderByDescending(r => r.RequestDate);
        }

        public async Task<CustomerModificationRequestDto> AddModificationRequestAsync(CustomerModificationRequestDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Step 1: Create appointment with default Amount = 0
            var appointment = new Appointment
            {
                UserId = dto.UserId,
                VehicleId = dto.VehicleId,
                Type = Models.Entities.Type.Modifications,
                Status = Models.Entities.AppointmentStatus.Pending,
                DateTime = dto.RequestDate,
                StartDateTime = dto.RequestDate,
                EndDateTime = dto.RequestDate.AddHours(1),
                Amount = 0 // initial amount is 0 until admin approves
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Step 2: Create modification request
            var request = new ModificationRequest
            {
                Title = dto.Title,
                Description = dto.Description,
                AppointmentId = appointment.AppointmentId
            };

            await _requestRepository.AddAsync(request);

            // Step 3: Map and return DTO
            return MapToDto(request);
        }

        // Map entity to DTO including Amount
        private CustomerModificationRequestDto MapToDto(ModificationRequest request)
        {
            var appointment = request.Appointment;
            var vehicleRegNo = appointment?.CustomerVehicle?.RegistrationNumber ?? "N/A";

            return new CustomerModificationRequestDto
            {
                ModificationId = request.ModificationId,
                Title = request.Title,
                Description = request.Description,
                VehicleId = appointment?.VehicleId ?? 0,
                VehicleRegistrationNumber = vehicleRegNo,
                CreatedDate = appointment?.DateTime ?? DateTime.UtcNow,
                RequestStatus = appointment?.Status.ToString() ?? "Pending",
                AppointmentId = request.AppointmentId,
                UserId = appointment?.UserId ?? 0,
                RequestDate = appointment?.DateTime ?? DateTime.UtcNow,
                Amount = appointment?.Amount ?? 0 // include the amount
            };
        }
    }
}
