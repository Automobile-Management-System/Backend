using automobile_backend.Models.DTO;

namespace automobile_backend.InterFaces.IServices
{
    public interface IEmployeeTimeLogService
    {
        Task<PaginatedResponse<EmployeeTimeLogDTO>> GetEmployeeLogsAsync(
            int employeeId,
            int pageNumber,
            int pageSize,
            string? search
        );
    }
}
