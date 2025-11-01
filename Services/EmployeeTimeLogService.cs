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

        public async Task<PaginatedResponse<EmployeeTimeLogDTO>> GetEmployeeTimeLogsAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search,
            DateTime? startDate,
            DateTime? endDate)
        {
            var (logs, totalCount) = await _repository.GetEmployeeTimeLogsAsync(userId, pageNumber, pageSize, search, startDate, endDate);

            var result = logs.Select(t => new EmployeeTimeLogDTO
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

            return new PaginatedResponse<EmployeeTimeLogDTO>
            {
                Data = result,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
