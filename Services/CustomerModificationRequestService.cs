//using automobile_backend.InterFaces.IServices;
//using automobile_backend.InterFaces.IRepository;
//using automobile_backend.Models.DTOs;
//using automobile_backend.Models.Entities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace automobile_backend.Services
//{
//    public class CustomerModificationRequestService : ICustomerModificationRequestService
//    {
//        private readonly ICustomerModificationRequestRepository _requestRepository;
//        private readonly ApplicationDbContext _context;

//        public CustomerModificationRequestService(
//            ICustomerModificationRequestRepository requestRepository,
//            ApplicationDbContext context)
//        {
//            _requestRepository = requestRepository;
//            _context = context;
//        }

//        public async Task<IEnumerable<CustomerModificationRequestDto>> GetAllModificationRequestsAsync()
//        {
//            var requests = await _requestRepository.GetAllAsync();

//            // Sort by CreatedDate descending (newest first)
//            return requests
//                .Select(MapToDto)
//                .OrderByDescending(r => r.RequestDate);
//        }

//        public async Task<IEnumerable<CustomerModificationRequestDto>> GetByUserIdAsync(int userId)
//        {
//            var requests = await _requestRepository.GetByUserIdAsync(userId);

//            // Sort by CreatedDate descending (newest first)
//            return requests
//                .Select(MapToDto)
//                .OrderByDescending(r => r.RequestDate);
//        }

//        public async Task<CustomerModificationRequestDto> AddModificationRequestAsync(CustomerModificationRequestDto dto)
//        {
//            if (dto == null) throw new ArgumentNullException(nameof(dto));

//            // Step 1: Create appointment with default Amount = 0
//            var appointment = new Appointment
//            {
//                UserId = dto.UserId,
//                VehicleId = dto.VehicleId,
//                Type = Models.Entities.Type.Modifications,
//                Status = Models.Entities.AppointmentStatus.Pending,
//                DateTime = dto.RequestDate,
//                StartDateTime = dto.RequestDate,
//                EndDateTime = dto.RequestDate.AddHours(1),
//                Amount = 0 // initial amount is 0 until admin approves
//            };

//            _context.Appointments.Add(appointment);
//            await _context.SaveChangesAsync();

//            // Step 2: Create modification request
//            var request = new ModificationRequest
//            {
//                Title = dto.Title,
//                Description = dto.Description,
//                AppointmentId = appointment.AppointmentId
//            };

//            await _requestRepository.AddAsync(request);

//            // Step 3: Map and return DTO
//            return MapToDto(request);
//        }

//        // Map entity to DTO including Amount
//        private CustomerModificationRequestDto MapToDto(ModificationRequest request)
//        {
//            var appointment = request.Appointment;
//            var vehicleRegNo = appointment?.CustomerVehicle?.RegistrationNumber ?? "N/A";

//            return new CustomerModificationRequestDto
//            {
//                ModificationId = request.ModificationId,
//                Title = request.Title,
//                Description = request.Description,
//                VehicleId = appointment?.VehicleId ?? 0,
//                VehicleRegistrationNumber = vehicleRegNo,
//                CreatedDate = appointment?.DateTime ?? DateTime.UtcNow,
//                RequestStatus = appointment?.Status.ToString() ?? "Pending",
//                AppointmentId = request.AppointmentId,
//                UserId = appointment?.UserId ?? 0,
//                RequestDate = appointment?.DateTime ?? DateTime.UtcNow,
//                Amount = appointment?.Amount ?? 0 // include the amount
//            };
//        }
//    }
//}


using automobile_backend.InterFaces.IServices;
using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using automobile_backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
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
        private readonly IServiceProvider _serviceProvider;

        public CustomerModificationRequestService(
            ICustomerModificationRequestRepository requestRepository,
            ApplicationDbContext context,
            IServiceProvider serviceProvider)
        {
            _requestRepository = requestRepository;
            _context = context;
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<CustomerModificationRequestDto>> GetAllModificationRequestsAsync()
        {
            var requests = await _requestRepository.GetAllAsync();
            return requests
                .Select(MapToDto)
                .OrderByDescending(r => r.RequestDate);
        }

        public async Task<IEnumerable<CustomerModificationRequestDto>> GetByUserIdAsync(int userId)
        {
            var requests = await _requestRepository.GetByUserIdAsync(userId);
            return requests
                .Select(MapToDto)
                .OrderByDescending(r => r.RequestDate);
        }

        public async Task<CustomerModificationRequestDto> AddModificationRequestAsync(CustomerModificationRequestDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // Step 1: Create appointment
            var appointment = new Appointment
            {
                UserId = dto.UserId,
                VehicleId = dto.VehicleId,
                Type = Models.Entities.Type.Modifications,
                Status = Models.Entities.AppointmentStatus.Pending,
                DateTime = dto.RequestDate,
                StartDateTime = dto.RequestDate,
                EndDateTime = dto.RequestDate.AddHours(1),
                Amount = 0
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

            // Step 3: Fetch the complete request with all navigation properties for notification
            var completeRequest = await _context.ModificationRequests
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.User)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.CustomerVehicle)
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.EmployeeAppointments)
                        .ThenInclude(ea => ea.User)
                .FirstOrDefaultAsync(r => r.ModificationId == request.ModificationId);

            // Step 4: Map DTO for response
            var createdDto = MapToDto(request);

            // ---- REAL-TIME NOTIFICATION TO ADMINS ----
            // Create notification object matching frontend ModificationRequest interface
            if (completeRequest != null)
            {
                var notificationData = new
                {
                    modificationId = completeRequest.ModificationId,
                    modificationName = completeRequest.Title,
                    description = completeRequest.Description,
                    userName = completeRequest.Appointment?.User != null
                        ? $"{completeRequest.Appointment.User.FirstName} {completeRequest.Appointment.User.LastName}"
                        : "Unknown",
                    vehicleNumber = completeRequest.Appointment?.CustomerVehicle?.RegistrationNumber ?? "N/A",
                    status = completeRequest.Appointment?.Status.ToString() ?? "Pending",
                    dateTime = completeRequest.Appointment?.DateTime ?? DateTime.UtcNow,
                    amount = completeRequest.Appointment?.Amount ?? 0,
                    assignee = completeRequest.Appointment?.EmployeeAppointments?.FirstOrDefault()?.User != null
                        ? $"{completeRequest.Appointment.EmployeeAppointments.FirstOrDefault().User.FirstName} {completeRequest.Appointment.EmployeeAppointments.FirstOrDefault().User.LastName}"
                        : "Unassigned",
                    appointmentId = completeRequest.AppointmentId
                };

                var hubContext = _serviceProvider.GetRequiredService<IHubContext<AdminNotificationHub>>();
                await hubContext.Clients
                    .Group(AdminNotificationHub.AdminGroup)
                    .SendAsync("NewModificationRequest", notificationData);
            }

            return createdDto;
        }

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
                Amount = appointment?.Amount ?? 0
            };
        }
    }
}