using automobile_backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IServiceAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAllServiceAppointmentsAsync();
        Task<Appointment?> GetServiceAppointmentByIdAsync(int appointmentId);
        Task<IEnumerable<User>> GetAvailableEmployeesAsync(DateTime date, SlotsTime slotTime);
        Task<Appointment> UpdateAppointmentAsync(Appointment appointment);
        Task<IEnumerable<Appointment>> GetAppointmentsByEmployeeAndDateAsync(int employeeId, DateTime date);

    }
}