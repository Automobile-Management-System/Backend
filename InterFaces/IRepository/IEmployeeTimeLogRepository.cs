using automobile_backend.Models.DTO;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IEmployeeTimeLogRepository
    {
        Task<PaginatedResponse<EmployeeTimeLogDTO>> GetEmployeeLogsAsync(
            int employeeId,
            int pageNumber,
            int pageSize,
            string? search
        );
    }
}
