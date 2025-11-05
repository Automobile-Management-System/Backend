using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IEmployeeServiceWorkService
    {
        Task<IEnumerable<TimeLog>> GetEmployeeWorkAsync();

        // ✅ Added
        Task<IEnumerable<object>> GetEmployeeAssignedAppointmentCountsAsync();
         Task<IEnumerable<object>> GetAllEmployeesWithDailyAssignmentCountAsync(DateTime date);
    }
}
