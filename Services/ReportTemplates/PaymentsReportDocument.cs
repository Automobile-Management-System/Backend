using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace automobile_backend.Services.ReportTemplates
{
    public class PaymentsReportDocument : IDocument
    {
        private readonly IEnumerable<AdminPaymentDetailDto> _payments;
        private readonly string? _search;
        private readonly PaymentStatus? _status;
        private readonly PaymentMethod? _method;

        public PaymentsReportDocument(IEnumerable<AdminPaymentDetailDto> payments, string? search, PaymentStatus? status, PaymentMethod? method)
        {
            _payments = payments;
            _search = search;
            _status = status;
            _method = method;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(30f); // <-- Add 'f'
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Text("AutoMobile Services - Payments Report")
                   .SemiBold().FontSize(20f).FontColor(Colors.Blue.Medium); // <-- Add 'f'

                col.Item().Text($"Generated on: {System.DateTime.Now:yyyy-MM-dd HH:mm}")
                   .FontSize(9f); // <-- Add 'f'

                col.Item().PaddingTop(10f).Text("Filters Applied:", TextStyle.Default.SemiBold()); // <-- Add 'f'
                col.Item().Text(string.IsNullOrWhiteSpace(_search) ? "Search: None" : $"Search: '{_search}'", TextStyle.Default.FontSize(9f)); // <-- Add 'f'
                col.Item().Text(_status.HasValue ? $"Status: {_status.Value}" : "Status: All", TextStyle.Default.FontSize(9f)); // <-- Add 'f'
                col.Item().Text(_method.HasValue ? $"Method: {_method.Value}" : "Method: All", TextStyle.Default.FontSize(9f)); // <-- Add 'f'
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(20f).Column(col => // <-- Add 'f'
            {
                col.Item().Element(ComposeTable);
                col.Item().Element(ComposeSummary);
            });
        }

        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35f);  // ID <-- Add 'f'
                    columns.RelativeColumn(2f); // Customer <-- Add 'f'
                    columns.RelativeColumn(1.5f); // Email <-- Add 'f'
                    columns.RelativeColumn(1.5f); // Service/Mod <-- Add 'f'
                    columns.RelativeColumn(1f);   // Amount <-- Add 'f'
                    columns.RelativeColumn(1f);   // Status <-- Add 'f'
                    columns.RelativeColumn(1f);   // Method <-- Add 'f'
                    columns.RelativeColumn(1f);   // Date <-- Add 'f'
                });

                // Header
                table.Header(header =>
                {
                    // --- FIX: Change IContainerCell to IContainer ---
                    static IContainer CellStyle(IContainer container) =>
                        container.BorderBottom(1f).BorderColor(Colors.Grey.Lighten2).Padding(5f).Background(Colors.Grey.Lighten4); // <-- Add 'f'

                    header.Cell().Element(CellStyle).Text("ID").SemiBold();
                    header.Cell().Element(CellStyle).Text("Customer").SemiBold();
                    header.Cell().Element(CellStyle).Text("Email").SemiBold();
                    header.Cell().Element(CellStyle).Text("Service/Mod").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Amount").SemiBold();
                    header.Cell().Element(CellStyle).Text("Status").SemiBold();
                    header.Cell().Element(CellStyle).Text("Method").SemiBold();
                    header.Cell().Element(CellStyle).Text("Date").SemiBold();
                });

                // Rows
                foreach (var payment in _payments)
                {
                    // --- FIX: Change IContainerCell to IContainer ---
                    static IContainer CellStyle(IContainer container) =>
                        container.BorderBottom(1f).BorderColor(Colors.Grey.Lighten3).Padding(4f); // <-- Add 'f'

                    table.Cell().Element(CellStyle).Text($"PAY-{payment.PaymentId}");
                    table.Cell().Element(CellStyle).Text($"{payment.CustomerFirstName} {payment.CustomerLastName}");
                    table.Cell().Element(CellStyle).Text(payment.CustomerEmail);
                    table.Cell().Element(CellStyle).Text(payment.AppointmentType);
                    table.Cell().Element(CellStyle).AlignRight().Text($"LKR{payment.Amount:N2}");
                    table.Cell().Element(CellStyle).Text(payment.Status);
                    table.Cell().Element(CellStyle).Text(payment.PaymentMethod);
                    table.Cell().Element(CellStyle).Text(payment.PaymentDateTime.ToShortDateString());
                }
            });
        }

        void ComposeSummary(IContainer container)
        {
            container.PaddingTop(20f).AlignRight().Column(col => // <-- Add 'f'
            {
                var totalRevenue = _payments.Where(p => p.Status == "Completed").Sum(p => p.Amount);
                var totalPending = _payments.Where(p => p.Status == "Pending").Sum(p => p.Amount);

                col.Item().Text($"Total Payments: {_payments.Count()}").SemiBold();
                col.Item().Text($"Total Completed Revenue: LKR{totalRevenue:N2}").SemiBold().FontColor(Colors.Green.Medium);
                col.Item().Text($"Total Pending Amount: LKR{totalPending:N2}").SemiBold().FontColor(Colors.Orange.Medium);
            });
        }
    }
}