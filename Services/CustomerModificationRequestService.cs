using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class CustomerModificationRequestService : ICustomerModificationRequestService
    {
        private readonly ICustomerModificationRequestRepository _requestRepository;

        public CustomerModificationRequestService(ICustomerModificationRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }

        // Get all modification requests
        public async Task<IEnumerable<CustomerModificationRequestDto>> GetAllModificationRequestsAsync()
        {
            var requests = await _requestRepository.GetAllAsync();
            return requests.Select(MapToDto).ToList();
        }

        // Add new modification request - default status: Pending
        public async Task AddModificationRequestAsync(CustomerModificationRequestDto modificationRequestDto)
        {
            var entity = new ModificationRequest
            {
                Title = modificationRequestDto.Title,
                Description = modificationRequestDto.Description,
                AppointmentId = modificationRequestDto.AppointmentId,

                // ✅ Set default status using enum
                Status = ModificationStatus.Pending
            };

            await _requestRepository.AddAsync(entity);
        }

        // Get requests by user ID
        public async Task<IEnumerable<CustomerModificationRequestDto>> GetByUserIdAsync(int userId)
        {
            var requests = await _requestRepository.GetByUserIdAsync(userId);
            return requests.Select(MapToDto).ToList();
        }

        // Map entity to DTO
        private CustomerModificationRequestDto MapToDto(ModificationRequest r)
        {
            var createdDateUtc = r.Appointment.DateTime.ToUniversalTime();

            // Convert enum to string for frontend
            string status = r.Status.ToString();

            return new CustomerModificationRequestDto
            {
                ModificationId = r.ModificationId,
                Title = r.Title,
                Description = r.Description,
                VehicleId = r.Appointment.VehicleId,
                CreatedDate = createdDateUtc,
                RequestStatus = status,
                AppointmentId = r.AppointmentId,
                AppointmentSummary = $"Appointment #{r.AppointmentId} – {r.Appointment.Type}"
            };
        }
    }
}
