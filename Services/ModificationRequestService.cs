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
        private readonly IAddOnRepository _addOnRepository;

        public ModificationRequestService(
            IModificationRequestRepository modificationRepository,
            IPaymentRepository paymentRepository,
            IAddOnRepository addOnRepository)
        {
            _modificationRepository = modificationRepository;
            _paymentRepository = paymentRepository;
            _addOnRepository = addOnRepository;
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
                priority = "medium",
                status = r.Status.ToString().ToLower(),
                adminResponse = r.AdminResponse,
                createdAt = r.CreatedAt
            });
        }

        public async Task<object?> ReviewModificationRequestAsync(int id, ReviewRequestDto reviewDto)
        {
            var request = await _modificationRepository.GetByIdAsync(id);
            
            if (request == null)
            {
                return null;
            }

            // Update modification request
            request.AdminResponse = reviewDto.AdminResponse;

            if (reviewDto.Action.ToLower() == "approve")
            {
                request.Status = ModificationStatus.Approved;

                // Create AddOn if estimated cost is provided
                if (reviewDto.EstimatedCost.HasValue && reviewDto.EstimatedCost.Value > 0)
                {
                    var payment = await _paymentRepository.GetByAppointmentIdAsync(request.AppointmentId);
                    
                    if (payment != null)
                    {
                        // Create new AddOn
                        var addOn = new AddOn
                        {
                            Description = $"Modification: {request.Title}",
                            Amount = reviewDto.EstimatedCost.Value,
                            PaymentId = payment.PaymentId
                        };

                        await _addOnRepository.CreateAsync(addOn);

                        // Update payment total
                        payment.Amount += reviewDto.EstimatedCost.Value;
                        await _paymentRepository.UpdateAsync(payment);
                    }
                }
            }
            else if (reviewDto.Action.ToLower() == "reject")
            {
                request.Status = ModificationStatus.Rejected;
            }

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
                status = request.Status.ToString().ToLower(),
                estimatedCost = reviewDto.EstimatedCost ?? 0,
                adminResponse = request.AdminResponse,
                createdAt = request.CreatedAt
            };
        }
    }
}