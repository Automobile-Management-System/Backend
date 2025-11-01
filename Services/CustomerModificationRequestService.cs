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
            return requests.Select(MapToDto);
        }

        public async Task<IEnumerable<CustomerModificationRequestDto>> GetByUserIdAsync(int userId)
        {
            var requests = await _requestRepository.GetByUserIdAsync(userId);
            return requests.Select(MapToDto);
        }

        public async Task AddModificationRequestAsync(CustomerModificationRequestDto dto)
        {
            // Step 1: Create appointment automatically
            var appointment = new Appointment
            {
                UserId = dto.UserId,
                VehicleId = dto.VehicleId,
                Type = Models.Entities.Type.Modifications
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Step 2: Create modification request
            var request = new ModificationRequest
            {
                Title = dto.Title,
                Description = dto.Description,
                AppointmentId = appointment.AppointmentId,
                Status = ModificationStatus.Pending
            };

            await _requestRepository.AddAsync(request);
        }

        private CustomerModificationRequestDto MapToDto(ModificationRequest request)
        {
            return new CustomerModificationRequestDto
            {
                ModificationId = request.ModificationId,
                Title = request.Title,
                Description = request.Description,
                VehicleId = request.Appointment?.VehicleId ?? 0,
                CreatedDate = DateTime.UtcNow, // DTO will format automatically
                RequestStatus = request.Status.ToString(),
                AppointmentId = request.AppointmentId,
                UserId = request.Appointment?.UserId ?? 0
            };
        }
    }
}
