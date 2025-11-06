using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.DTO;
using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.Repository
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.CustomerVehicle)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(aps => aps.Service)
                .ToListAsync();
        }

        public async Task<Appointment> AddAsync(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Ensure navigation properties needed by responses are available
            await _context.Entry(appointment).Reference(a => a.CustomerVehicle).LoadAsync();

            return appointment;
        }

        public async Task<PaginatedResponse<Appointment>> GetPaginatedAsync(
     int userId,
     int pageNumber,
     int pageSize,
     AppointmentStatus? status)
        {
            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.CustomerVehicle)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(aps => aps.Service)
                .Where(a => a.UserId == userId)  
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(a => a.DateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponse<Appointment>
            {
                Data = data,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }


    }
}
