using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace automobile_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminAnalyticsController : ControllerBase
    {
        private readonly IServiceAnalyticsService _service;

        public AdminAnalyticsController(IServiceAnalyticsService service)
        {
            _service = service;
        }

        [HttpGet("services")]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            var services = await _service.GetServicesAsync();
            return Ok(services);
        }

        [HttpGet("overview")]
        public async Task<ActionResult<AnalyticsOverviewDto>> GetOverview()
        {
            var overview = await _service.GetOverviewAsync();
            return Ok(overview);
        }

        [HttpGet("service-completion-rates")]
        public async Task<ActionResult<IEnumerable<ServiceCompletionDto>>> GetServiceCompletionRates()
        {
            var rates = await _service.GetServiceCompletionRatesAsync();
            return Ok(rates);
        }

        [HttpGet("employee-performance")]
        public async Task<ActionResult<IEnumerable<EmployeePerformanceDto>>> GetEmployeePerformance()
        {
            var performance = await _service.GetEmployeePerformanceAsync();
            return Ok(performance);
        }

        [HttpGet("revenue-stats")]
        public async Task<ActionResult<RevenueStatsDto>> GetRevenueStats()
        {
            var revenue = await _service.GetRevenueStatsAsync();
            return Ok(revenue);
        }

        [HttpGet("revenue-trend")]
        public async Task<ActionResult<RevenueTrendDto>> GetRevenueTrend()
        {
            var trend = await _service.GetRevenueTrendAsync();
            return Ok(trend);
        }

        [HttpGet("customer-activity")]
        public async Task<ActionResult<CustomerActivityDto>> GetCustomerActivity()
        {
            var activity = await _service.GetCustomerActivityAsync();
            return Ok(activity);
        }

        [HttpGet("generate-report")]
        public async Task<IActionResult> GenerateReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var pdfBytes = await _service.GenerateReportAsync(startDate, endDate);
            return File(pdfBytes, "application/pdf", "AnalyticsReport.pdf");
        }
    }
}