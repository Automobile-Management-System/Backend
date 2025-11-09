using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTO;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TypeEnum = automobile_backend.Models.Entities.Type; // avoid clash with System.Type

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
                    Capacity = SlotCapacity
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

    // Validate user
    var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
    if (!userExists) throw new Exception("User not found.");

    // Validate slot
    if (!AllowedSlots.Contains(dto.SlotsTime)) throw new Exception("Selected time slot is not available.");

    // Validate vehicle
    var vehicleExists = await _context.CustomerVehicles
        .AnyAsync(v => v.VehicleId == dto.VehicleId && v.UserId == userId);
    if (!vehicleExists) throw new Exception("Vehicle not found for this user.");

    // Validate services
    var services = await _context.Services
        .Where(s => dto.ServiceIds.Contains(s.ServiceId))
        .ToListAsync();
    if (services.Count == 0) throw new Exception("No valid services found.");

    // Check slot capacity
    var (start, end) = GetSlotRange(day, dto.SlotsTime);
    var currentCount = await _context.Appointments
        .Where(a => a.DateTime.Date == day && a.SlotsTime == dto.SlotsTime && a.Status != AppointmentStatus.Rejected)
        .CountAsync();
    if (currentCount >= SlotCapacity) throw new Exception("Selected time slot is fully booked.");

    // Calculate total amount
    var totalAmount = services.Sum(s => (decimal?)s.BasePrice ?? 0m);

    // Create appointment entity
    var appointment = new Appointment
    {
        UserId = userId,
        VehicleId = dto.VehicleId,
        DateTime = start,
        SlotsTime = dto.SlotsTime,
        StartDateTime = start,
        EndDateTime = end,
        Status = AppointmentStatus.Pending,
        Type = TypeEnum.Service,
        Amount = totalAmount,
        AppointmentServices = services.Select(s => new automobile_backend.Models.Entities.AppointmentService
        {
            ServiceId = s.ServiceId
        }).ToList()
    };

    var saved = await _appointmentRepository.AddAsync(appointment);

    // ✅ Broadcast WebSocket notification
    try
    {
        var notification = new
        {
            type = "NEW_APPOINTMENT",
            message = $"New appointment booked by user ID {userId}",
            appointmentId = saved.AppointmentId,
            date = saved.DateTime.ToString("yyyy-MM-dd HH:mm"),
            vehicleId = saved.VehicleId,
            totalAmount = saved.Amount
        };

        await automobile_backend.WebSockets.WebSocketHandler.BroadcastObjectAsync(notification);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WebSocket Notification Error] {ex.Message}");
    }

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

        public async Task<PaginatedResponse<Appointment>> GetPaginatedAppointmentsAsync(
    int userId, int pageNumber, int pageSize, AppointmentStatus? status)
        {
            return await _appointmentRepository
                .GetPaginatedAsync(userId, pageNumber, pageSize, status);
        }


    }
}
