using automobile_backend.Models.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace automobile_backend.Services.InvoiceTemplates
{
    public class InvoiceDocument : IDocument
    {
        private readonly Payment _payment;

        public InvoiceDocument(Payment payment)
        {
            _payment = payment;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);

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
            var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("AutoMobile Services").Style(titleStyle);

                    column.Item().Text(text =>
                    {
                        text.Span("Invoice ").SemiBold();
                        text.Span($"INV-{_payment.PaymentId:D5}");
                    });

                    column.Item().Text(text =>
                    {
                        text.Span("Date Issued: ").SemiBold();
                        text.Span(_payment.PaymentDateTime.ToLocalTime().ToString("yyyy-MM-dd"));
                    });
                });

                row.ConstantItem(100).Height(50).Placeholder(); // Placeholder for a logo
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                // Bill To
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Bill To").SemiBold().FontSize(14);
                        var user = _payment.Appointment.User;
                        col.Item().Text($"{user.FirstName} {user.LastName}");
                        col.Item().Text(user.Email);
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("From").SemiBold().FontSize(14);
                        col.Item().Text("AutoMobile Services");
                        col.Item().Text("123 Auto Street, Colombo");
                        col.Item().Text("contact@automobile.com");
                    });
                });

                // Invoice Table
                column.Item().PaddingTop(25).Element(ComposeTable);

                // Total
                column.Item().AlignRight().PaddingTop(10).Text($"Total: LKR{_payment.Amount:F2}")
                    .SemiBold().FontSize(16);
                
                // Status
                column.Item().AlignRight().Text($"Status: {_payment.Status}")
                    .FontColor(Colors.Green.Medium).SemiBold();
            });
        }

        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Description
                    columns.RelativeColumn(1); // Unit Price
                    columns.RelativeColumn(1); // Quantity
                    columns.RelativeColumn(1); // Total
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Service").SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Unit Price").SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text("Quantity").SemiBold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Total").SemiBold();
                });

                // Item Row
                var serviceType = _payment.Appointment.Type.ToString();
                var amount = _payment.Amount;

                table.Cell().Padding(5).Text($"Appointment: {serviceType}");
                table.Cell().Padding(5).AlignRight().Text($"LKR{amount:F2}");
                table.Cell().Padding(5).AlignCenter().Text("1");
                table.Cell().Padding(5).AlignRight().Text($"LKR{amount:F2}");
            });
        }
    }
}