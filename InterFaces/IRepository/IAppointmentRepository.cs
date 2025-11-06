using automobile_backend.Models.DTO;
using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAllAsync();
        Task<Appointment> AddAsync(Appointment appointment);

        Task<PaginatedResponse<Appointment>> GetPaginatedAsync(
    int userId, int pageNumber, int pageSize, AppointmentStatus? status);

    }
}
