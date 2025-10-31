using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IServices
{
    public interface IServiceAnalyticsService
    {
        Task<IEnumerable<Service>> GetServicesAsync();
        Task<AnalyticsOverviewDto> GetOverviewAsync();
        Task<IEnumerable<ServiceCompletionDto>> GetServiceCompletionRatesAsync();
        Task<IEnumerable<EmployeePerformanceDto>> GetEmployeePerformanceAsync();
        Task<RevenueStatsDto> GetRevenueStatsAsync();
        Task<CustomerActivityDto> GetCustomerActivityAsync();
        Task<byte[]> GenerateReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
