using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
            return await _context.ModificationRequests.ToListAsync();
        }
    }
}
