using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTO;

namespace automobile_backend.Services
{
    public class EmployeeTimeLogService : IEmployeeTimeLogService
    {
        private readonly IEmployeeTimeLogRepository _repository;

        public EmployeeTimeLogService(IEmployeeTimeLogRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<EmployeeTimeLogDTO>> GetEmployeeTimeLogsAsync(int userId)
        {
            var logs = await _repository.GetEmployeeTimeLogsAsync(userId);

            return logs.Select(t => new EmployeeTimeLogDTO
            {
                LogId = t.LogId,
                StartDateTime = t.StartDateTime,
                EndDateTime = t.EndDateTime,
                HoursLogged = t.HoursLogged,
                IsActive = t.IsActive,
                Notes = t.Notes,
                CustomerName = $"{t.Appointment.User.FirstName} {t.Appointment.User.LastName}",
                Services = t.Appointment.AppointmentServices
                    .Select(s => s.Service.ServiceName)
                    .ToList(),
                Modifications = t.Appointment.ModificationRequests
                    .Select(m => m.Title)
                    .ToList()
            }).ToList();
        }
    }
}
