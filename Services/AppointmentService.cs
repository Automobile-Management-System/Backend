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
        private const int SlotCapacity = 5;

        private static readonly SlotsTime[] AllowedSlots =
        {
            SlotsTime.EightAm,  // 08:00-10:00
            SlotsTime.TenAm,    // 10:00-12:00
            SlotsTime.OnePm,    // 13:00-15:00
            SlotsTime.ThreePm   // 15:00-17:00
        };

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

        public async Task<IEnumerable<SlotAvailabilityDto>> GetSlotAvailabilityAsync(DateTime date)
        {
            var day = date.Date;

            // Count only appointments that actually occupy capacity (exclude rejected)
            var bookings = await _context.Appointments
                .Where(a => a.DateTime.Date == day && a.Status != AppointmentStatus.Rejected)
                .GroupBy(a => a.SlotsTime)
                .Select(g => new { Slot = g.Key, Count = g.Count() })
                .ToListAsync();

            var bookedLookup = bookings.ToDictionary(b => b.Slot, b => b.Count);

            var result = AllowedSlots
                .Select(slot => new SlotAvailabilityDto
                {
                    Slot = slot,
                    Booked = bookedLookup.TryGetValue(slot, out var count) ? count : 0,
                    Capacity = SlotCapacity // ensure API consumers see the true capacity
                })
                .ToList();

            return result;
        }

        public async Task<IReadOnlyList<VehicleOptionDto>> GetUserVehicleOptionsAsync(int userId)
        {
            return await _context.CustomerVehicles
                .Where(v => v.UserId == userId)
                .OrderBy(v => v.RegistrationNumber)
                .Select(v => new VehicleOptionDto
                {
                    VehicleId = v.VehicleId,
                    RegistrationNumber = v.RegistrationNumber
                })
                .ToListAsync();
        }

        public async Task<Appointment> CreateAppointmentAsync(int userId, CreateServiceAppointmentDto dto)
        {
            var day = dto.AppointmentDateTime.Date;

            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
                throw new Exception("User not found.");

            if (!AllowedSlots.Contains(dto.SlotsTime))
                throw new Exception("Selected time slot is not available.");

            var vehicleExists = await _context.CustomerVehicles
                .AnyAsync(v => v.VehicleId == dto.VehicleId && v.UserId == userId);
            if (!vehicleExists)
                throw new Exception("Vehicle not found for this user.");

            var services = await _context.Services
                .Where(s => dto.ServiceIds.Contains(s.ServiceId))
                .ToListAsync();

            if (services.Count == 0)
                throw new Exception("No valid services found.");

            var (start, end) = GetSlotRange(day, dto.SlotsTime);

            // Enforce capacity at booking time (exclude rejected)
            var currentCount = await _context.Appointments
                .Where(a => a.DateTime.Date == day && a.SlotsTime == dto.SlotsTime && a.Status != AppointmentStatus.Rejected)
                .CountAsync();

            if (currentCount >= SlotCapacity)
                throw new Exception("Selected time slot is fully booked.");

            var appointment = new Appointment
            {
                UserId = userId,
                VehicleId = dto.VehicleId,
                DateTime = start, // ✅ Use start instead of day
                SlotsTime = dto.SlotsTime,
                StartDateTime = start,
                EndDateTime = end,
                Status = AppointmentStatus.Pending,
                AppointmentServices = new List<automobile_backend.Models.Entities.AppointmentService>()
            };

            foreach (var service in services)
            {
                appointment.AppointmentServices.Add(new automobile_backend.Models.Entities.AppointmentService
                {
                    Appointment = appointment,
                    ServiceId = service.ServiceId
                });
            }

            var saved = await _appointmentRepository.AddAsync(appointment);
            return saved;
        }

        private static (DateTime start, DateTime end) GetSlotRange(DateTime date, SlotsTime slot)
        {
            return slot switch
            {
                SlotsTime.EightAm => (date.AddHours(8), date.AddHours(10)),
                SlotsTime.TenAm => (date.AddHours(10), date.AddHours(12)),
                SlotsTime.OnePm => (date.AddHours(13), date.AddHours(15)),
                SlotsTime.ThreePm => (date.AddHours(15), date.AddHours(17)),
                _ => throw new ArgumentOutOfRangeException(nameof(slot), "Unsupported time slot.")
            };
        }
    }
}
