using automobile_backend.Models.Entities;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IAddOnRepository
    {
        // ... existing methods from other team members ...

        // ADD THIS METHOD:
        Task<AddOn> CreateAsync(AddOn addOn);
    }
}