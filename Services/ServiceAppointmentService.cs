using automobile_backend.InterFaces.IServices;
using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class ServiceAppointmentService : IServiceAppointmentService
    {
        private readonly IServiceAppointmentRepository _appointmentRepository;

        public ServiceAppointmentService(IServiceAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<(IEnumerable<object> Data, int TotalCount)> GetAllServiceAppointmentsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var appointments = await _appointmentRepository.GetAllServiceAppointmentsAsync();
            var totalCount = appointments.Count();

            var pagedAppointments = appointments
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    appointmentId = a.AppointmentId,
                    dateTime = a.DateTime,
                    slotTime = a.SlotsTime.ToString(),
                    status = a.Status.ToString(),
                    amount = a.Amount,
                    customerName = a.User != null
                        ? $"{a.User.FirstName} {a.User.LastName}"
                        : "Unknown",
                    vehicleNumber = a.CustomerVehicle?.RegistrationNumber ?? "N/A",
                    services = (a.AppointmentServices?
                        .Select(aps => new
                        {
                            serviceId = aps.ServiceId,
                            serviceName = aps.Service?.ServiceName ?? "Unknown",
                            basePrice = aps.Service?.BasePrice ?? 0
                        }) ?? Enumerable.Empty<dynamic>())
                        .ToList(),
                    assignedEmployees = (a.EmployeeAppointments?
                        .Where(ea => ea.User != null)
                        .Select(ea => new
                        {
                            employeeId = ea.UserId,
                            employeeName = $"{ea.User.FirstName} {ea.User.LastName}"
                        }) ?? Enumerable.Empty<dynamic>())
                        .ToList()
                });

            return (pagedAppointments, totalCount);
        }

        public async Task<object?> GetServiceAppointmentByIdAsync(int appointmentId)
        {
            var appointment = await _appointmentRepository.GetServiceAppointmentByIdAsync(appointmentId);
            if (appointment == null) return null;

            var services = (appointment.AppointmentServices?
                                .Select(aps => new
                                {
                                    serviceId = aps.ServiceId,
                                    serviceName = aps.Service?.ServiceName ?? "Unknown",
                                    description = aps.Service?.Description ?? "",
                                    basePrice = aps.Service?.BasePrice ?? 0
                                }) ?? Enumerable.Empty<dynamic>())
                            .ToList();

            var assignedEmployees = (appointment.EmployeeAppointments?
                                        .Where(ea => ea.User != null)
                                        .Select(ea => new
                                        {
                                            employeeId = ea.UserId,
                                            employeeName = $"{ea.User.FirstName} {ea.User.LastName}",
                                            email = ea.User.Email
                                        }) ?? Enumerable.Empty<dynamic>())
                                    .ToList();

            return new
            {
                appointmentId = appointment.AppointmentId,
                dateTime = appointment.DateTime,
                slotTime = appointment.SlotsTime.ToString(),
                status = appointment.Status.ToString(),
                amount = appointment.Amount,
                startDateTime = appointment.StartDateTime,
                endDateTime = appointment.EndDateTime,
                customerName = appointment.User != null
                    ? $"{appointment.User.FirstName} {appointment.User.LastName}"
                    : "Unknown",
                customerEmail = appointment.User?.Email ?? "N/A",
                customerPhone = appointment.User?.PhoneNumber ?? "N/A",
                vehicleNumber = appointment.CustomerVehicle?.RegistrationNumber ?? "N/A",
                vehicleModel = appointment.CustomerVehicle?.Model ?? "N/A",
                services = services,
                assignedEmployees = assignedEmployees
            };
        }

        public async Task<IEnumerable<object>> GetAvailableEmployeesForAssignmentAsync(DateTime date, SlotsTime slotTime)
        {
            var availableEmployees = await _appointmentRepository.GetAvailableEmployeesAsync(date, slotTime);

            return availableEmployees.Select(e => new
            {
                employeeId = e.UserId,
                employeeName = $"{e.FirstName} {e.LastName}",
                email = e.Email,
                phoneNumber = e.PhoneNumber
            });
        }

        public async Task<object?> AssignEmployeeToAppointmentAsync(int appointmentId, int employeeId)
{
    // Get the appointment
    var appointment = await _appointmentRepository.GetServiceAppointmentByIdAsync(appointmentId);
    if (appointment == null) return null;

    // Only allow assignment if appointment is Pending
    if (appointment.Status != AppointmentStatus.Pending) // Pending = 0
        throw new Exception("Only pending appointments can be assigned.");

    // Initialize EmployeeAppointments if null
    appointment.EmployeeAppointments ??= new List<EmployeeAppointment>();

    // Check if employee is already assigned
    var isAlreadyAssigned = appointment.EmployeeAppointments.Any(ea => ea.UserId == employeeId);
    if (isAlreadyAssigned)
        throw new Exception("Employee is already assigned to this appointment");

    // Assign the employee
    appointment.EmployeeAppointments.Add(new EmployeeAppointment
    {
        AppointmentId = appointmentId,
        UserId = employeeId
    });

    // Update status to Upcoming
    appointment.Status = AppointmentStatus.Upcoming; // Upcoming = 1

    // Save changes
    await _appointmentRepository.UpdateAppointmentAsync(appointment);

    // Prepare response
    var assignedEmployees = (appointment.EmployeeAppointments?
                                .Where(ea => ea.User != null)
                                .Select(ea => new
                                {
                                    employeeId = ea.UserId,
                                    employeeName = $"{ea.User.FirstName} {ea.User.LastName}"
                                }) ?? Enumerable.Empty<dynamic>())
                            .ToList();

    return new
    {
        appointmentId = appointment.AppointmentId,
        status = appointment.Status.ToString(),
        assignedEmployees = assignedEmployees
    };
}


        public async Task<IEnumerable<object>> GetEmployeeAssignmentsAsync(int employeeId, DateTime date)
{
    var appointments = await _appointmentRepository.GetAppointmentsByEmployeeAndDateAsync(employeeId, date);

    return appointments.Select(a => new
    {
        appointmentId = a.AppointmentId,
        customerName = $"{a.User.FirstName} {a.User.LastName}",
        vehicleNumber = a.CustomerVehicle?.RegistrationNumber ?? "N/A",
        dateTime = a.DateTime,
        slotTime = a.SlotsTime.ToString(),
        status = a.Status.ToString(),
        services = a.AppointmentServices?
                    .Select(aps => aps.Service?.ServiceName ?? "Unknown")
                    .ToList() ?? new List<string>()
    });
}

    }
}
