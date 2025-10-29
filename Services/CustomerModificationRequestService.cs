using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Entities;
using automobile_backend.Models.DTOs;


namespace automobile_backend.Services
{
    public class CustomerModificationRequestService : ICustomerModificationRequestService
    {
        private readonly ICustomerModificationRequestRepository _requestRepository;

        public CustomerModificationRequestService(ICustomerModificationRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }

        public async Task<IEnumerable<CustomerModificationRequestDto>> GetAllModificationRequestsAsync()
        {
            var requests = await _requestRepository.GetAllAsync();

            var dtos = requests.Select(r =>
            {
                var createdDateUtc = r.Appointment.DateTime.ToUniversalTime();

                return new CustomerModificationRequestDto
                {
                    ModificationId = r.ModificationId,
                    Title = r.Title,
                    Description = r.Description,
                    VehicleId = r.Appointment.VehicleId,
                    CreatedDate = createdDateUtc,
                    RequestStatus = r.Appointment.Status.ToString(),
                    AppointmentId = r.AppointmentId,
                    AppointmentSummary = $"Appointment #{r.AppointmentId} – {r.Appointment.Type}"
                };
            }).ToList();

            return dtos;
        }

        // ✅ Fixed method to match interface
        public async Task AddModificationRequestAsync(CustomerModificationRequestDto modificationRequestDto)
        {
            var entity = new ModificationRequest
            {
                Title = modificationRequestDto.Title,
                Description = modificationRequestDto.Description,
                AppointmentId = modificationRequestDto.AppointmentId
            };

            await _requestRepository.AddAsync(entity);
        }
    }
}