using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Repository
{
    public class ServiceAppointmentRepository : IServiceAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceAppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Appointment>> GetAllServiceAppointmentsAsync()
        {
            return await _context.Appointments
                .Include(a => a.User) // Customer
                .Include(a => a.CustomerVehicle) // Vehicle
                .Include(a => a.AppointmentServices)
                    .ThenInclude(aps => aps.Service) // Services
                .Include(a => a.EmployeeAppointments)
                    .ThenInclude(ea => ea.User) // Assigned employees
                .Where(a => a.Type == Models.Entities.Type.Service) // Type = 1 for Service
                .Where(a => a.User.Role == Enums.Customer) // Role = 2 for Customer
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();
        }

        public async Task<Appointment?> GetServiceAppointmentByIdAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.User) // Customer
                .Include(a => a.CustomerVehicle) // Vehicle
                .Include(a => a.AppointmentServices)
                    .ThenInclude(aps => aps.Service) // Services
                .Include(a => a.EmployeeAppointments)
                    .ThenInclude(ea => ea.User) // Assigned employees
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.Type == Models.Entities.Type.Service);
        }

       public async Task<IEnumerable<User>> GetAvailableEmployeesAsync(DateTime date, SlotsTime slotTime)
{
    // 1. Get all employees
    var allEmployees = await _context.Users
        .Where(u => u.Role == Enums.Employee)
        .ToListAsync();

    // 2. Get employees already assigned that day & slot
    var assignedEmployeeIds = await _context.EmployeeAppointments
        .Include(ea => ea.Appointment)
        .Where(ea => ea.Appointment.DateTime.Date == date.Date 
                  && ea.Appointment.SlotsTime == slotTime)
        .Select(ea => ea.UserId)
        .Distinct()
        .ToListAsync();

    // 3. Keep only employees NOT in that list (available)
    var availableEmployees = allEmployees
        .Where(e => !assignedEmployeeIds.Contains(e.UserId))
        .ToList();

    return availableEmployees;
}

        public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }
        public async Task<IEnumerable<Appointment>> GetAppointmentsByEmployeeAndDateAsync(int employeeId, DateTime date)
{
    return await _context.EmployeeAppointments
        .Include(ea => ea.Appointment)
            .ThenInclude(a => a.User) // Customer
        .Include(ea => ea.Appointment)
            .ThenInclude(a => a.CustomerVehicle)
        .Include(ea => ea.Appointment)
            .ThenInclude(a => a.AppointmentServices)
                .ThenInclude(aps => aps.Service)
        .Where(ea => ea.UserId == employeeId && ea.Appointment.DateTime.Date == date.Date)
        .Select(ea => ea.Appointment)
        .ToListAsync();
}

    }
}
