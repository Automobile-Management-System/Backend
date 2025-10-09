using automobile_backend.Models.Entities;

namespace automobile_backend.InterFaces.IServices
{
    public interface IEmployeeServiceWorkService
    {
        Task<IEnumerable<TimeLog>> GetEmployeeWorkAsync();
    }
}
