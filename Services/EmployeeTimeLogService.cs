using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTO;

public class EmployeeTimeLogService : IEmployeeTimeLogService
{
    private readonly IEmployeeTimeLogRepository _repository;

    public EmployeeTimeLogService(IEmployeeTimeLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResponse<EmployeeTimeLogDTO>> GetEmployeeLogsAsync(
        int employeeId, int pageNumber, int pageSize, string? search)
    {
        return await _repository.GetEmployeeLogsAsync(employeeId, pageNumber, pageSize, search);
    }
}
