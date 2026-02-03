using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EDA.INFRAESTRUCTURE.Services
{
    public class ShiftReportPdfService : IShiftReportPdfService
    {
        static ShiftReportPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateShiftReportPdf(ShiftReportData data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ContinuousSize(80, Unit.Millimetre);
                    page.MarginVertical(3, Unit.Millimetre);
                    page.MarginHorizontal(2, Unit.Millimetre);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Content().Column(column =>
                    {
                        column.Spacing(2);

                        // === HEADER - Empresa ===
                        column.Item().AlignCenter().Text(data.CompanyName)
                            .Bold().FontSize(10);

                        if (!string.IsNullOrEmpty(data.CompanyAddress))
                            column.Item().AlignCenter().Text(data.CompanyAddress).FontSize(7);

                        if (!string.IsNullOrEmpty(data.CompanyRTN))
                            column.Item().AlignCenter().Text($"RTN: {data.CompanyRTN}").FontSize(7);

                        // Separador
                        column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // === TITULO ===
                        column.Item().AlignCenter().Text("REPORTE DE CIERRE DE TURNO")
                            .Bold().FontSize(9);

                        // Separador
                        column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // === INFO TURNO ===
                        column.Item().Text($"Usuario: {data.UserName}").FontSize(8);
                        column.Item().Text($"Turno: {data.ShiftType}").FontSize(8);
                        column.Item().Text($"Inicio: {data.StartTime:dd/MM/yyyy HH:mm}").FontSize(8);
                        column.Item().Text($"Cierre: {data.EndTime:dd/MM/yyyy HH:mm}").FontSize(8);

                        // Separador
                        column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // === SALDO ESPERADO ===
                        column.Item().Text("SALDO ESPERADO").Bold().FontSize(8);
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("  Saldo Inicial:").FontSize(8);
                            row.ConstantItem(80).AlignRight().Text($"L {data.InitialAmount:N2}").FontSize(8);
                        });
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("  (+) Efectivo vendido:").FontSize(8);
                            row.ConstantItem(80).AlignRight().Text($"L {data.ExpectedCash:N2}").FontSize(8);
                        });
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("  (+) Tarjeta vendido:").FontSize(8);
                            row.ConstantItem(80).AlignRight().Text($"L {data.ExpectedCard:N2}").FontSize(8);
                        });
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("  Total esperado:").Bold().FontSize(8);
                            row.ConstantItem(80).AlignRight().Text($"L {data.ExpectedAmount:N2}").Bold().FontSize(8);
                        });

                        // Separador
                        column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // === SALDO REPORTADO ===
                        column.Item().Text("SALDO REPORTADO").Bold().FontSize(8);
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("  Saldo Inicial:").FontSize(8);
                            row.ConstantItem(80).AlignRight().Text($"L {data.InitialAmount:N2}").FontSize(8);
                        });
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("  (+) Efectivo reportado:").FontSize(8);
                            row.ConstantItem(80).AlignRight().Text($"L {data.FinalCashAmount:N2}").FontSize(8);
                        });
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("  (+) Tarjeta reportado:").FontSize(8);
                            row.ConstantItem(80).AlignRight().Text($"L {data.FinalCardAmount:N2}").FontSize(8);
                        });
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("  Total reportado:").Bold().FontSize(8);
                            row.ConstantItem(80).AlignRight().Text($"L {data.FinalAmount:N2}").Bold().FontSize(8);
                        });

                        // Separador
                        column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // === DIFERENCIA ===
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("DIFERENCIA:").Bold().FontSize(9);
                            row.ConstantItem(80).AlignRight().Text($"L {data.Difference:N2}").Bold().FontSize(9);
                        });

                        // Separador
                        column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // === VENTAS ===
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Total Facturas:").FontSize(8);
                            row.ConstantItem(80).AlignRight().Text($"{data.TotalInvoices}").FontSize(8);
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Total Ventas:").Bold().FontSize(9);
                            row.ConstantItem(80).AlignRight().Text($"L {data.TotalSales:N2}").Bold().FontSize(9);
                        });

                        // Separador final
                        column.Item().PaddingVertical(4).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // Pie
                        column.Item().AlignCenter().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").FontSize(6);
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
