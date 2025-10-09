using automobile_backend.Models.Entities;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IEmployeeServiceWorkRepository
    {
        Task<IEnumerable<TimeLog>> GetEmployeeWorkAsync();
    }
}
