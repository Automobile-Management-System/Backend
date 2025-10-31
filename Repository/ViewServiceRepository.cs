using automobile_backend.Models.Entities;
using automobile_backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace automobile_backend.Repositories
{
    public class ViewServiceRepository : IViewServiceRepository
    {
        private readonly ApplicationDbContext _context;

        public ViewServiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Service> Services, int TotalCount)> GetAllViewServicesAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = _context.Services.AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string lowerSearch = searchTerm.ToLower();
                query = query.Where(s =>
                    s.ServiceName.ToLower().Contains(lowerSearch) ||
                    s.Description.ToLower().Contains(lowerSearch));
            }

            var totalCount = await query.CountAsync();

            var services = await query
                .OrderBy(s => s.ServiceId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (services, totalCount);
        }
    }
}
