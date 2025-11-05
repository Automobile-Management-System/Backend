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

        public async Task<RevenueTrendDto> GetRevenueTrendAsync()
        {
            return await _repository.GetRevenueTrendAsync();
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
            var revenueTrend = await GetRevenueTrendAsync();
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
                        .Text("Automobile Service Center - Analytics Report")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            // Overview Section
                            x.Item().Text("Overview").FontSize(16).Bold();
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(200);
                                    columns.ConstantColumn(150);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Metric");
                                    header.Cell().Element(CellStyle).Text("Value");
                                });

                                table.Cell().Element(CellStyle).Text("Total Revenue (Year to Date)");
                                table.Cell().Element(CellStyle).Text($"${overview.TotalRevenue:F2}");

                                table.Cell().Element(CellStyle).Text("Total Appointments");
                                table.Cell().Element(CellStyle).Text(overview.TotalAppointments.ToString());

                                table.Cell().Element(CellStyle).Text("Average Revenue per Month");
                                table.Cell().Element(CellStyle).Text($"${overview.AverageRevenuePerMonth:F2}");

                                table.Cell().Element(CellStyle).Text("Growth Rate (Month over Month)");
                                table.Cell().Element(CellStyle).Text($"{overview.GrowthRate:F2}%");
                            });

                            // Employee Performance Section
                            x.Item().Text("Employee Performance").FontSize(16).Bold();
                            x.Item().Text("Completed appointments and revenue generated").FontSize(10).Italic();
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
                                    header.Cell().Element(CellStyle).Text("Completed");
                                    header.Cell().Element(CellStyle).Text("Revenue");
                                    header.Cell().Element(CellStyle).Text("Rating");
                                });

                                foreach (var emp in employeePerf)
                                {
                                    table.Cell().Element(CellStyle).Text(emp.EmployeeName);
                                    table.Cell().Element(CellStyle).Text(emp.CompletedAppointments.ToString());
                                    table.Cell().Element(CellStyle).Text($"${emp.RevenueGenerated:F2}");
                                    table.Cell().Element(CellStyle).Text($"? {emp.AverageRating:F1}");
                                }
                            });

                            // Monthly Revenue Comparison
                            x.Item().Text("Monthly Revenue Comparison").FontSize(16).Bold();
                            x.Item().Text("Revenue by month").FontSize(10).Italic();
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(150);
                                    columns.ConstantColumn(150);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Month");
                                    header.Cell().Element(CellStyle).Text("Revenue ($)");
                                });

                                foreach (var rev in revenue.RevenueByMonth)
                                {
                                    table.Cell().Element(CellStyle).Text(rev.Key);
                                    table.Cell().Element(CellStyle).Text($"${rev.Value:F2}");
                                }
                            });

                            // Revenue Trend
                            x.Item().Text("Revenue Trend").FontSize(16).Bold();
                            x.Item().Text("Monthly revenue and appointment volume").FontSize(10).Italic();
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(120);
                                    columns.ConstantColumn(120);
                                    columns.ConstantColumn(120);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Month");
                                    header.Cell().Element(CellStyle).Text("Revenue ($)");
                                    header.Cell().Element(CellStyle).Text("Appointments");
                                });

                                foreach (var month in revenueTrend.RevenueByMonth.Keys)
                                {
                                    table.Cell().Element(CellStyle).Text(month);
                                    table.Cell().Element(CellStyle).Text($"${revenueTrend.RevenueByMonth[month]:F2}");
                                    table.Cell().Element(CellStyle).Text(revenueTrend.AppointmentsByMonth.GetValueOrDefault(month, 0).ToString());
                                }
                            });

                            // Service Distribution
                            x.Item().Text("Service Distribution").FontSize(16).Bold();
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

                            // Customer Activity
                            x.Item().Text("Customer Activity").FontSize(16).Bold();
                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(250);
                                    columns.ConstantColumn(100);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Metric");
                                    header.Cell().Element(CellStyle).Text("Value");
                                });

                                table.Cell().Element(CellStyle).Text("Total Customers");
                                table.Cell().Element(CellStyle).Text(customerAct.TotalCustomers.ToString());

                                table.Cell().Element(CellStyle).Text("Active Customers (Last Month)");
                                table.Cell().Element(CellStyle).Text(customerAct.ActiveCustomers.ToString());

                                table.Cell().Element(CellStyle).Text("Avg Appointments per Customer");
                                table.Cell().Element(CellStyle).Text($"{customerAct.AverageAppointmentsPerCustomer:F2}");

                                table.Cell().Element(CellStyle).Text("Average Customer Rating");
                                table.Cell().Element(CellStyle).Text($"? {customerAct.AverageRating:F2}");
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
