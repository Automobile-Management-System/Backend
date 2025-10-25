using automobile_backend.InterFaces.IRepository;
using automobile_backend.Models.Entities;
using System.Threading.Tasks;

namespace automobile_backend.Repository
{
    public class AddOnRepository : IAddOnRepository
    {
        private readonly ApplicationDbContext _context;

        public AddOnRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ... existing methods from other team members ...

        // ADD THIS METHOD:
        public async Task<AddOn> CreateAsync(AddOn addOn)
        {
            _context.AddOns.Add(addOn);
            await _context.SaveChangesAsync();
            return addOn;
        }
    }
}