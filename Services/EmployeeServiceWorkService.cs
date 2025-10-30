using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        // ✅ Added: Fetch assigned appointment counts
        public async Task<IEnumerable<object>> GetEmployeeAssignedAppointmentCountsAsync()
        {
            return await _employeeWorkRepository.GetEmployeeAssignedAppointmentCountsAsync();
        }
    }
}
