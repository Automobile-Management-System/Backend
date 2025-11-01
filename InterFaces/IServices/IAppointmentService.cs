using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
        Task<PaginatedResult<Appointment>> GetAllAppointmentsPaginatedAsync(PaginationParameters parameters);
        Task<PaginatedResult<Appointment>> GetUserAppointmentsPaginatedAsync(int userId, PaginationParameters parameters);
        Task<Appointment> CreateAppointmentAsync(int userId, CreateServiceAppointmentDto dto);
        Task<IEnumerable<SlotAvailabilityDto>> GetSlotAvailabilityAsync(DateTime date);
        Task<IReadOnlyList<VehicleOptionDto>> GetUserVehicleOptionsAsync(int userId);
    }
}
