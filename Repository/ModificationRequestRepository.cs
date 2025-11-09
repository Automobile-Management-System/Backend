using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Repository
{
    public class ModificationRequestRepository : IModificationRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public ModificationRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task AddAsync(CreateModificationRequestDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ModificationRequest>> GetAllAsync()
{
    return await _context.ModificationRequests
        .Include(m => m.Appointment)
            .ThenInclude(a => a.User) // Customer
        .Include(m => m.Appointment)
            .ThenInclude(a => a.CustomerVehicle) // Vehicle
        .Include(m => m.Appointment)
            .ThenInclude(a => a.EmployeeAppointments)
                .ThenInclude(ea => ea.User) // Assignee/Employee
        .Where(m => m.Appointment.Type == Models.Entities.Type.Modifications) // fully qualified
        .Where(m => m.Appointment.User.Role == Enums.Customer) // Role = 2
        .ToListAsync();
}

        public async Task<ModificationRequest?> GetByIdAsync(int id)
        {
            return await _context.ModificationRequests
                .Include(m => m.Appointment)
                    .ThenInclude(a => a.User)
                .Include(m => m.Appointment)
                    .ThenInclude(a => a.CustomerVehicle)
                .Include(m => m.Appointment)
                    .ThenInclude(a => a.EmployeeAppointments)
                        .ThenInclude(ea => ea.User)
                .FirstOrDefaultAsync(m => m.ModificationId == id);
        }

        public async Task<ModificationRequest> UpdateAsync(ModificationRequest request)
        {
            _context.ModificationRequests.Update(request);
            await _context.SaveChangesAsync();
            return request;
        }
    }
}