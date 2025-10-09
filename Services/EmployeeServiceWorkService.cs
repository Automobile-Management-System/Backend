using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Entities;

namespace automobile_backend.Services
{
    public class EmployeeServiceWorkService : IEmployeeServiceWorkService
    {
        private readonly IEmployeeServiceWorkRepository _employeeWorkRepository;

        public EmployeeServiceWorkService(IEmployeeServiceWorkRepository employeeWorkRepository)
        {
            _employeeWorkRepository = employeeWorkRepository;
        }

        public async Task<IEnumerable<TimeLog>> GetEmployeeWorkAsync()
        {
            return await _employeeWorkRepository.GetEmployeeWorkAsync();
        }
    }
}
