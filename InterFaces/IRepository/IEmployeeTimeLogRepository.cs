using automobile_backend.Models.Entities;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IEmployeeTimeLogRepository
    {
        Task<(List<TimeLog> logs, int totalCount)> GetEmployeeTimeLogsAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search,
            DateTime? startDate,
            DateTime? endDate);
    }
}
