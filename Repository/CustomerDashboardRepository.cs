using automobile_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

public class CustomerDashboardRepository : ICustomerDashboardRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerDashboardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    //public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(int userId)
    //{
    //    // TODO: Implement DB query to get upcoming appointments
    //    throw new NotImplementedException();
    //}


}