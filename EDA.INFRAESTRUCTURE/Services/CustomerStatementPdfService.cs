using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EDA.INFRAESTRUCTURE.Services
{
    public class CustomerStatementPdfService : ICustomerStatementPdfService
    {
        static CustomerStatementPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateCustomerStatementPdf(CustomerStatementPdfData data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => CreateHeader(c, data));
                    page.Content().PaddingVertical(10).Element(c => CreateContent(c, data));
                    page.Footer().Element(CreateFooter);
                });
            });

            return document.GeneratePdf();
        }

        private void CreateHeader(IContainer container, CustomerStatementPdfData data)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    // Logo o nombre de empresa
                    if (data.CompanyLogo != null && data.CompanyLogo.Length > 0)
                    {
                        row.ConstantItem(80).Image(data.CompanyLogo);
                        row.ConstantItem(10);
                    }

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(data.CompanyName).Bold().FontSize(14);
                        if (!string.IsNullOrEmpty(data.CompanyRtn))
                            c.Item().Text($"RTN: {data.CompanyRtn}").FontSize(9);
                        if (!string.IsNullOrEmpty(data.CompanyAddress))
                            c.Item().Text(data.CompanyAddress).FontSize(9);
                        if (!string.IsNullOrEmpty(data.CompanyPhone))
                            c.Item().Text($"Tel: {data.CompanyPhone}").FontSize(9);
                    });

                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().AlignRight().Text("ESTADO DE CUENTA").Bold().FontSize(14);
                        c.Item().AlignRight().Text($"Fecha: {data.GeneratedAt:dd/MM/yyyy}").FontSize(9);
                    });
                });

                column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                // Datos del cliente
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("CLIENTE:").Bold().FontSize(10);
                        c.Item().Text(data.CustomerName).FontSize(10);
                        if (!string.IsNullOrEmpty(data.CustomerRtn))
                            c.Item().Text($"RTN: {data.CustomerRtn}").FontSize(9);
                        if (!string.IsNullOrEmpty(data.CustomerCompany))
                            c.Item().Text(data.CustomerCompany).FontSize(9);
                    });
                });

                column.Item().PaddingTop(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
            });
        }

        private void CreateContent(IContainer container, CustomerStatementPdfData data)
        {
            container.Column(column =>
            {
                column.Spacing(10);

                // Tarjetas de resumen
                column.Item().Row(row =>
                {
                    row.RelativeItem().Element(c => CreateSummaryCard(c, "Total Adeudado", $"L {data.TotalOwed:N2}"));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => CreateSummaryCard(c, "Facturas Pendientes", data.PendingInvoicesCount.ToString()));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => CreateSummaryCard(c, "Monto Vencido", $"L {data.OverdueAmount:N2}", Colors.Red.Medium));
                });

                // Tabla de facturas
                column.Item().PaddingTop(10).Text("DETALLE DE FACTURAS PENDIENTES").Bold().FontSize(11);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.5f);  // # Factura
                        columns.RelativeColumn(1.2f);  // Fecha Emision
                        columns.RelativeColumn(1.2f);  // Fecha Vencimiento
                        columns.RelativeColumn(1.2f);  // Total
                        columns.RelativeColumn(1.2f);  // Saldo Pendiente
                        columns.RelativeColumn(0.8f);  // Dias Vencido
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellHeaderStyle).Text("# Factura");
                        header.Cell().Element(CellHeaderStyle).Text("Emision");
                        header.Cell().Element(CellHeaderStyle).Text("Vencimiento");
                        header.Cell().Element(CellHeaderStyle).AlignRight().Text("Total");
                        header.Cell().Element(CellHeaderStyle).AlignRight().Text("Saldo");
                        header.Cell().Element(CellHeaderStyle).AlignCenter().Text("Dias");
                    });

                    foreach (var invoice in data.Invoices)
                    {
                        var isOverdue = invoice.DaysOverdue > 0;

                        if (isOverdue)
                        {
                            table.Cell().Element(CellOverdueStyle).Text(invoice.InvoiceNumber);
                            table.Cell().Element(CellOverdueStyle).Text(invoice.IssueDate.ToString("dd/MM/yyyy"));
                            table.Cell().Element(CellOverdueStyle).Text(invoice.DueDate?.ToString("dd/MM/yyyy") ?? "-");
                            table.Cell().Element(CellOverdueStyle).AlignRight().Text($"L {invoice.Total:N2}");
                            table.Cell().Element(CellOverdueStyle).AlignRight().Text($"L {invoice.OutstandingAmount:N2}");
                            table.Cell().Element(CellOverdueStyle).AlignCenter().Text($"+{invoice.DaysOverdue}");
                        }
                        else
                        {
                            table.Cell().Element(CellStyle).Text(invoice.InvoiceNumber);
                            table.Cell().Element(CellStyle).Text(invoice.IssueDate.ToString("dd/MM/yyyy"));
                            table.Cell().Element(CellStyle).Text(invoice.DueDate?.ToString("dd/MM/yyyy") ?? "-");
                            table.Cell().Element(CellStyle).AlignRight().Text($"L {invoice.Total:N2}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"L {invoice.OutstandingAmount:N2}");
                            table.Cell().Element(CellStyle).AlignCenter().Text(invoice.DaysOverdue.ToString());
                        }
                    }

                    // Fila de total
                    table.Cell().ColumnSpan(4).Element(CellTotalStyle).Text("TOTAL");
                    table.Cell().Element(CellTotalStyle).AlignRight().Text($"L {data.TotalOwed:N2}");
                    table.Cell().Element(CellTotalStyle).Text("");
                });

                // Nota informativa
                column.Item().PaddingTop(20).Text(text =>
                {
                    text.Span("Nota: ").Bold();
                    text.Span("Los montos con dias positivos (+) indican facturas vencidas. ");
                    text.Span("Por favor realizar el pago lo antes posible para evitar recargos.");
                });
            });
        }

        private static void CreateSummaryCard(IContainer container, string label, string value, string? color = null)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4)
                .Padding(10).Column(column =>
                {
                    column.Item().Text(label).FontSize(9).FontColor(Colors.Grey.Darken2);
                    column.Item().Text(value).Bold().FontSize(16).FontColor(color ?? Colors.Black);
                });
        }

        private static void CreateFooter(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text("Gracias por su preferencia").FontSize(8).FontColor(Colors.Grey.Darken1);
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        }

        private static IContainer CellHeaderStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.Bold().FontSize(9))
                .PaddingVertical(5)
                .PaddingHorizontal(3)
                .Background(Colors.Grey.Lighten2)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Medium);
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.FontSize(9))
                .PaddingVertical(4)
                .PaddingHorizontal(3)
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten1);
        }

        private static IContainer CellOverdueStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Red.Darken1))
                .PaddingVertical(4)
                .PaddingHorizontal(3)
                .Background(Colors.Red.Lighten5)
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten1);
        }

        private static IContainer CellTotalStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.Bold().FontSize(9))
                .PaddingVertical(5)
                .PaddingHorizontal(3)
                .Background(Colors.Grey.Lighten3)
                .BorderTop(1)
                .BorderColor(Colors.Grey.Medium);
        }
    }
}
