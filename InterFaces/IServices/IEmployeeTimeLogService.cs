using automobile_backend.Models.DTO;

namespace automobile_backend.InterFaces.IServices
{
    public interface IEmployeeTimeLogService
    {
        Task<List<EmployeeTimeLogDTO>> GetEmployeeTimeLogsAsync(int userId);
    }
}
