using automobile_backend.InterFaces.IRepository;
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

        public async Task<IEnumerable<ModificationRequest>> GetAllAsync()
        {
            return await _context.ModificationRequests
                .Include(m => m.Appointment)
                    .ThenInclude(a => a.User)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<ModificationRequest?> GetByIdAsync(int id)
        {
            return await _context.ModificationRequests
                .Include(m => m.Appointment)
                    .ThenInclude(a => a.User)
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