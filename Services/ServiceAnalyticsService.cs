using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.Services
{
    public class ServiceAnalyticsService : IServiceAnalyticsService
    {
        private readonly IServiceAnalyticsRepository _repository;

        public ServiceAnalyticsService(IServiceAnalyticsRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Service>> GetServicesAsync()
        {
            return await _repository.GetServicesAsync();
        }
    }
}
