using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Entities;
using automobile_backend.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class ModificationRequestService : IModificationRequestService
    {
        private readonly IModificationRequestRepository _requestRepository;

        public ModificationRequestService(IModificationRequestRepository requestRepository)
        {
            _requestRepository = requestRepository;
        }

        public async Task<IEnumerable<ModificationRequestDto>> GetAllModificationRequestsAsync()
        {
            var requests = await _requestRepository.GetAllAsync();

            var dtos = requests.Select(r =>
            {
                var createdDateUtc = r.Appointment.DateTime.ToUniversalTime();

                return new ModificationRequestDto
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
        public async Task AddModificationRequestAsync(ModificationRequestDto modificationRequestDto)
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
