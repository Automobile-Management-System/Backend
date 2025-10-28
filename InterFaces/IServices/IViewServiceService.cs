using automobile_backend.Models.DTOs;

namespace automobile_backend.Services.Interfaces
{
    public interface IViewServiceService
    {
        Task<PagedResult<ViewServiceDto>> GetAllViewServicesAsync(int pageNumber, int pageSize, string? searchTerm);
    }
}
