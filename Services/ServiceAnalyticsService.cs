using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.IO;
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

        public async Task<AnalyticsOverviewDto> GetOverviewAsync()
        {
            return await _repository.GetOverviewAsync();
        }

        public async Task<IEnumerable<ServiceCompletionDto>> GetServiceCompletionRatesAsync()
        {
            return await _repository.GetServiceCompletionRatesAsync();
        }

        public async Task<IEnumerable<EmployeePerformanceDto>> GetEmployeePerformanceAsync()
        {
            return await _repository.GetEmployeePerformanceAsync();
        }

        public async Task<RevenueStatsDto> GetRevenueStatsAsync()
        {
            return await _repository.GetRevenueStatsAsync();
        }

        public async Task<CustomerActivityDto> GetCustomerActivityAsync()
        {
            return await _repository.GetCustomerActivityAsync();
        }

        public async Task<byte[]> GenerateReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var overview = await GetOverviewAsync();
            var serviceRates = await GetServiceCompletionRatesAsync();
            var employeePerf = await GetEmployeePerformanceAsync();
            var revenue = await GetRevenueStatsAsync();
            var customerAct = await GetCustomerActivityAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("Automobile Management System - Analytics Report")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().Text("Overview").FontSize(16).Bold();
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(200);
                                    columns.ConstantColumn(100);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Metric");
                                    header.Cell().Element(CellStyle).Text("Value");
                                });

                                table.Cell().Element(CellStyle).Text("Total Appointments");
                                table.Cell().Element(CellStyle).Text(overview.TotalAppointments.ToString());

                                table.Cell().Element(CellStyle).Text("Completed Appointments");
                                table.Cell().Element(CellStyle).Text(overview.CompletedAppointments.ToString());

                                table.Cell().Element(CellStyle).Text("Total Revenue");
                                table.Cell().Element(CellStyle).Text($"${overview.TotalRevenue:F2}");

                                table.Cell().Element(CellStyle).Text("Total Customers");
                                table.Cell().Element(CellStyle).Text(overview.TotalCustomers.ToString());

                                table.Cell().Element(CellStyle).Text("Total Employees");
                                table.Cell().Element(CellStyle).Text(overview.TotalEmployees.ToString());
                            });

                            x.Item().Text("Service Completion Rates").FontSize(16).Bold();
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(150);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Service");
                                    header.Cell().Element(CellStyle).Text("Total");
                                    header.Cell().Element(CellStyle).Text("Completed");
                                    header.Cell().Element(CellStyle).Text("Rate (%)");
                                });

                                foreach (var rate in serviceRates)
                                {
                                    table.Cell().Element(CellStyle).Text(rate.ServiceName);
                                    table.Cell().Element(CellStyle).Text(rate.TotalAppointments.ToString());
                                    table.Cell().Element(CellStyle).Text(rate.CompletedAppointments.ToString());
                                    table.Cell().Element(CellStyle).Text($"{rate.CompletionRate:F2}");
                                }
                            });

                            x.Item().Text("Employee Performance").FontSize(16).Bold();
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(150);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Employee");
                                    header.Cell().Element(CellStyle).Text("Appointments");
                                    header.Cell().Element(CellStyle).Text("Avg Rating");
                                    header.Cell().Element(CellStyle).Text("Hours Logged");
                                });

                                foreach (var emp in employeePerf)
                                {
                                    table.Cell().Element(CellStyle).Text(emp.EmployeeName);
                                    table.Cell().Element(CellStyle).Text(emp.AppointmentsHandled.ToString());
                                    table.Cell().Element(CellStyle).Text($"{emp.AverageRating:F2}");
                                    table.Cell().Element(CellStyle).Text($"{emp.TotalHoursLogged:F2}");
                                }
                            });

                            x.Item().Text("Revenue Stats").FontSize(16).Bold();
                            x.Item().Text($"Total Revenue: ${revenue.TotalRevenue:F2}");
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Month");
                                    header.Cell().Element(CellStyle).Text("Revenue");
                                });

                                foreach (var rev in revenue.RevenueByMonth)
                                {
                                    table.Cell().Element(CellStyle).Text(rev.Key);
                                    table.Cell().Element(CellStyle).Text($"${rev.Value:F2}");
                                }
                            });

                            x.Item().Text("Customer Activity").FontSize(16).Bold();
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(200);
                                    columns.ConstantColumn(100);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Metric");
                                    header.Cell().Element(CellStyle).Text("Value");
                                });

                                table.Cell().Element(CellStyle).Text("Total Customers");
                                table.Cell().Element(CellStyle).Text(customerAct.TotalCustomers.ToString());

                                table.Cell().Element(CellStyle).Text("Active Customers");
                                table.Cell().Element(CellStyle).Text(customerAct.ActiveCustomers.ToString());

                                table.Cell().Element(CellStyle).Text("Avg Appointments per Customer");
                                table.Cell().Element(CellStyle).Text($"{customerAct.AverageAppointmentsPerCustomer:F2}");

                                table.Cell().Element(CellStyle).Text("Average Rating");
                                table.Cell().Element(CellStyle).Text($"{customerAct.AverageRating:F2}");
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated on ");
                            x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).Bold();
                        });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        static IContainer CellStyle(IContainer container)
        {
            return container
                .Border(1)
                .Background(Colors.Grey.Lighten3)
                .Padding(5)
                .AlignCenter()
                .AlignMiddle();
        }
    }
}
