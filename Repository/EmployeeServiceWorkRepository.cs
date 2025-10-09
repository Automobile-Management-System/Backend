using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.Repository
{
    public class EmployeeServiceWorkRepository : IEmployeeServiceWorkRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeServiceWorkRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TimeLog>> GetEmployeeWorkAsync()
        {
            return await _context.TimeLogs.ToListAsync();
        }
    }
}
