using automobile_backend.Models.Entities;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IEmployeeTimeLogRepository
    {
        Task<List<TimeLog>> GetEmployeeTimeLogsAsync(int userId);
    }
}
