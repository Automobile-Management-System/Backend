using automobile_backend.Models.DTOs;
using automobile_backend.Repositories.Interfaces;
using automobile_backend.Services.Interfaces;

namespace automobile_backend.Services
{
    public class ViewServiceService : IViewServiceService
    {
        private readonly IViewServiceRepository _viewServiceRepository;

        public ViewServiceService(IViewServiceRepository viewServiceRepository)
        {
            _viewServiceRepository = viewServiceRepository;
        }

        public async Task<PagedResult<ViewServiceDto>> GetAllViewServicesAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var (services, totalCount) = await _viewServiceRepository.GetAllViewServicesAsync(pageNumber, pageSize, searchTerm);

            var dtoList = services.Select(s => new ViewServiceDto
            {
                ServiceName = s.ServiceName,
                Description = s.Description,
                BasePrice = s.BasePrice
            }).ToList();

            return new PagedResult<ViewServiceDto>
            {
                Items = dtoList,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
