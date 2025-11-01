using automobile_backend.Models.DTO;

namespace automobile_backend.InterFaces.IServices
{
    public interface IEmployeeTimeLogService
    {
        Task<PaginatedResponse<EmployeeTimeLogDTO>> GetEmployeeTimeLogsAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search,
            DateTime? startDate,
            DateTime? endDate);
    }
}
