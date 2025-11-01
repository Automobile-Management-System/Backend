using automobile_backend.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IServiceAppointmentService
    {
        Task<(IEnumerable<object> Data, int TotalCount)> GetAllServiceAppointmentsAsync(int pageNumber = 1, int pageSize = 10);
        Task<object?> GetServiceAppointmentByIdAsync(int appointmentId);
        Task<IEnumerable<object>> GetAvailableEmployeesForAssignmentAsync(DateTime date, SlotsTime slotTime);
        Task<object?> AssignEmployeeToAppointmentAsync(int appointmentId, int employeeId);
        Task<IEnumerable<object>> GetEmployeeAssignmentsAsync(int employeeId, DateTime date);

    }
}
