using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace automobile_backend.Repository
{
    public class CustomerModificationRequestRepository : ICustomerModificationRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerModificationRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ModificationRequest>> GetAllAsync()
        {
            return await _context.ModificationRequests
                                 .Include(m => m.Appointment)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<ModificationRequest>> GetByUserIdAsync(int userId)
        {
            return await _context.ModificationRequests
                                 .Include(m => m.Appointment)
                                 .Where(m => m.Appointment != null && m.Appointment.UserId == userId)
                                 .ToListAsync();
        }

        public async Task AddAsync(ModificationRequest modificationRequest)
        {
            _context.ModificationRequests.Add(modificationRequest);
            await _context.SaveChangesAsync();
        }
    }
}
