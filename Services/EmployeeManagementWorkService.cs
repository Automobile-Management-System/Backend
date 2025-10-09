using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class EmployeeManagementWorkService : IEmployeeManagementWorkService
    {
        private readonly IEmployeeManagementWorkRepository _repository;

        public EmployeeManagementWorkService(IEmployeeManagementWorkRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsAsync()
        {
            return await _repository.GetAppointmentsAsync();
        }


    }
}
