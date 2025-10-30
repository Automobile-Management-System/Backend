using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ApplicationDbContext _context;

        public AppointmentService(IAppointmentRepository appointmentRepository, ApplicationDbContext context)
        {
            _appointmentRepository = appointmentRepository;
            _context = context;
        }

        public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
        {
            return await _appointmentRepository.GetAllAsync();
        }

        public async Task<Appointment> CreateAppointmentAsync(int userId, CreateServiceAppointmentDto dto)
        {
            // Validate user
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
                throw new Exception("User not found.");

            // Validate services
            var services = await _context.Services
                .Where(s => dto.ServiceIds.Contains(s.ServiceId))
                .ToListAsync();

            if (!services.Any())
                throw new Exception("No valid services found.");

            // Create appointment
            var appointment = new Appointment
            {
                UserId = userId,
                DateTime = dto.AppointmentDateTime,
                Status = AppointmentStatus.Scheduled,
                AppointmentServices = new List<automobile_backend.Models.Entities.AppointmentService>()
            };

            // Link selected services
            foreach (var service in services)
            {
                appointment.AppointmentServices.Add(new automobile_backend.Models.Entities.AppointmentService
                {
                    Appointment = appointment,
                    ServiceId = service.ServiceId
                });
            }

            return await _appointmentRepository.AddAsync(appointment);
        }
    }
}
