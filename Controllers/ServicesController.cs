using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using automobile_backend.Models.Entities;

namespace automobile_backend.Controllers
{
    // DTO for creating/updating services (no navigation properties)
    public class ServiceDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
    }

    // Pagination response DTO
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Services?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<PagedResult<Service>>> GetServices(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Max limit

            var totalCount = await _context.Services.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var services = await _context.Services
                .OrderBy(s => s.ServiceName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<Service>
            {
                Items = services,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            };

            return result;
        }

        // GET: api/Services/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetService(int id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            return service;
        }

        // GET: api/Services/stats
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetServiceStats()
        {
            var totalServices = await _context.Services.CountAsync();
            var averagePrice = totalServices > 0 
                ? await _context.Services.AverageAsync(s => s.BasePrice) 
                : 0;

            return new
            {
                totalServices = totalServices,
                averagePrice = averagePrice
            };
        }

        // POST: api/Services
        [HttpPost]
        public async Task<ActionResult<Service>> CreateService(ServiceDto serviceDto)
        {
            var service = new Service
            {
                ServiceName = serviceDto.ServiceName,
                Description = serviceDto.Description,
                BasePrice = serviceDto.BasePrice
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetService), new { id = service.ServiceId }, service);
        }

        // PUT: api/Services/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, ServiceDto serviceDto)
        {
            var existingService = await _context.Services.FindAsync(id);
            if (existingService == null)
            {
                return NotFound();
            }

            existingService.ServiceName = serviceDto.ServiceName;
            existingService.Description = serviceDto.Description;
            existingService.BasePrice = serviceDto.BasePrice;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Services/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            
            if (service == null)
            {
                return NotFound();
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServiceId == id);
        }
    }
}