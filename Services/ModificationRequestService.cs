using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class ModificationRequestService : IModificationRequestService
    {
        private readonly IModificationRequestRepository _modificationRepository;
        private readonly IPaymentRepository _paymentRepository;

        public ModificationRequestService(
            IModificationRequestRepository modificationRepository,
            IPaymentRepository paymentRepository)
        {
            _modificationRepository = modificationRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<IEnumerable<object>> GetAllModificationRequestsAsync()
        {
            var requests = await _modificationRepository.GetAllAsync();

            return requests.Select(r => new
            {
                id = r.ModificationId,
                customerId = r.Appointment?.UserId ?? 0,
                customerName = r.Appointment?.User != null
                    ? $"{r.Appointment.User.FirstName} {r.Appointment.User.LastName}"
                    : "Unknown",
                appointmentId = r.AppointmentId,
                serviceType = "Vehicle Service",
                appointmentDate = r.Appointment?.DateTime ?? DateTime.Now,
                title = r.Title,
                description = r.Description,
                requestType = "modification",
                priority = "medium"
            });
        }

        public async Task<object?> ReviewModificationRequestAsync(int id, ReviewRequestDto reviewDto)
        {
            var request = await _modificationRepository.GetByIdAsync(id);

            if (request == null)
            {
                return null;
            }

            // Removed AdminResponse because it's not part of the entity anymore
            await _modificationRepository.UpdateAsync(request);

            return new
            {
                id = request.ModificationId,
                customerId = request.Appointment?.UserId ?? 0,
                customerName = request.Appointment?.User != null
                    ? $"{request.Appointment.User.FirstName} {request.Appointment.User.LastName}"
                    : "Unknown",
                appointmentId = request.AppointmentId,
                serviceType = "Vehicle Service",
                appointmentDate = request.Appointment?.DateTime ?? DateTime.Now,
                title = request.Title,
                description = request.Description,
                requestType = "modification",
                priority = "medium",
                estimatedCost = reviewDto.EstimatedCost ?? 0
            };
        }
    }
}
