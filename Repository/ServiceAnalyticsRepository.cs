using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using automobile_backend.Data;

namespace automobile_backend.Repository
{
    public class ServiceAnalyticsRepository : IServiceAnalyticsRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceAnalyticsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Service>> GetServicesAsync()
        {
            return await _context.Services.ToListAsync();
        }
    }
}
