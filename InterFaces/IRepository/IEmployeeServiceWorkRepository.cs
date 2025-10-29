using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IEmployeeServiceWorkRepository
    {
        Task<IEnumerable<TimeLog>> GetEmployeeWorkAsync();

        // ✅ Added
        Task<IEnumerable<object>> GetEmployeeAssignedAppointmentCountsAsync();
    }
}
