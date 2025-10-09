using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IServiceAnalyticsService
    {
        Task<IEnumerable<Service>> GetServicesAsync();
    }
}
