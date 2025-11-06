using automobile_backend.InterFaces.IServices;
using automobile_backend.InterFaces.IRepository;
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
        private readonly INotificationService _notifications;

        public ModificationRequestService(IModificationRequestRepository modificationRepository, INotificationService notifications)
        {
            _modificationRepository = modificationRepository;
            _notifications = notifications;
        }

        // Pagination support: returns data and total count
        public async Task<(IEnumerable<object> Data, int TotalCount)> GetAllModificationRequestsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var requests = await _modificationRepository.GetAllAsync();

            var totalCount = requests.Count();

            var pagedRequests = requests
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    modificationId = r.ModificationId,
                    modificationName = r.Title,
                    description = r.Description,
                    userName = r.Appointment?.User != null
                        ? $"{r.Appointment.User.FirstName} {r.Appointment.User.LastName}"
                        : "Unknown",
                    vehicleNumber = r.Appointment?.CustomerVehicle?.RegistrationNumber ?? "N/A",
                    status = r.Appointment?.Status.ToString() ?? "Unknown",
                    dateTime = r.Appointment?.DateTime ?? DateTime.MinValue,
                    amount = r.Appointment?.Amount ?? 0,
                    assignee = r.Appointment?.EmployeeAppointments?.FirstOrDefault()?.User != null
                        ? $"{r.Appointment.EmployeeAppointments.FirstOrDefault().User.FirstName} {r.Appointment.EmployeeAppointments.FirstOrDefault().User.LastName}"
                        : "Unassigned",
                    appointmentId = r.AppointmentId
                });

            return (pagedRequests, totalCount);
        }

     public async Task<object?> ReviewModificationRequestAsync(int id, ReviewRequestDto reviewDto)
{
    var request = await _modificationRepository.GetByIdAsync(id);

    if (request == null)
        return null;

    // Defensive check — ensure Appointment is loaded
    if (request.Appointment == null)
        throw new Exception("Appointment not found for this modification request.");

    // Ensure EmployeeAppointments is initialized
    request.Appointment.EmployeeAppointments ??= new List<EmployeeAppointment>();

    // ✅ Assign employee if AssigneeId is provided and status is pending
    if (request.Appointment.Status == AppointmentStatus.Pending && reviewDto.AssigneeId.HasValue)
    {
        var exists = request.Appointment.EmployeeAppointments
            .Any(ea => ea.UserId == reviewDto.AssigneeId.Value);

        if (!exists)
        {
            request.Appointment.EmployeeAppointments.Add(new EmployeeAppointment
            {
                AppointmentId = request.AppointmentId,
                UserId = reviewDto.AssigneeId.Value
            });
        }
    }

    // ✅ Update appointment status based on action
    if (!string.IsNullOrWhiteSpace(reviewDto.Action))
    {
        var action = reviewDto.Action.ToLower();

        if (action == "approve")
            request.Appointment.Status = AppointmentStatus.Upcoming;
        else if (action == "reject")
            request.Appointment.Status = AppointmentStatus.Rejected;
    }

    // ✅ Update estimated cost safely
    if (reviewDto.EstimatedCost.HasValue && reviewDto.EstimatedCost.Value > 0)
        request.Appointment.Amount += reviewDto.EstimatedCost.Value;

    await _modificationRepository.UpdateAsync(request);

    // 🔔 Send customer email based on the new status
    var statusForEmail = request.Appointment.Status == AppointmentStatus.Upcoming
        ? "Approved"
        : request.Appointment.Status == AppointmentStatus.Rejected
            ? "Rejected"
            : request.Appointment.Status.ToString();

    await _notifications.SendStatusUpdateAsync("Appointment", request.AppointmentId, statusForEmail);

    // ✅ Safely build response
    var assigneeNames = request.Appointment.EmployeeAppointments?
        .Where(ea => ea.User != null)
        .Select(ea => ea.User.FirstName + " " + ea.User.LastName)
        .ToList();

    return new
    {
        modificationId = request.ModificationId,
        modificationName = request.Title,
        description = request.Description,
        assignee = (assigneeNames != null && assigneeNames.Any())
            ? string.Join(", ", assigneeNames)
            : "Unassigned",
        appointmentId = request.AppointmentId,
        status = request.Appointment.Status.ToString(),
        amount = request.Appointment.Amount
    };
}


    }
}
