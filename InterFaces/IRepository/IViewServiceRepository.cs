using automobile_backend.Models.Entities;

namespace automobile_backend.Repositories.Interfaces
{
    public interface IViewServiceRepository
    {
        Task<(IEnumerable<Service> Services, int TotalCount)> GetAllViewServicesAsync(int pageNumber, int pageSize, string? searchTerm);
    }
}
