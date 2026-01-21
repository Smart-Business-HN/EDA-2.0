using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EDA.INFRAESTRUCTURE.Services
{
    public class InvoicePdfService : IInvoicePdfService
    {
        static InvoicePdfService()
        {
            // Configurar licencia Community (gratuita para uso comercial limitado)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateInvoicePdf(InvoicePdfData data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Configurar página para papel de 80mm (continuo)
                    page.ContinuousSize(80, Unit.Millimetre);
                    page.MarginVertical(3, Unit.Millimetre);
                    page.MarginHorizontal(2, Unit.Millimetre);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Content().Column(column =>
                    {
                        column.Spacing(2);

                        // === HEADER - Datos de la Empresa ===
                        column.Item().AlignCenter().Text(data.CompanyName)
                            .Bold().FontSize(10);

                        if (!string.IsNullOrEmpty(data.CompanyAddress1))
                            column.Item().AlignCenter().Text(data.CompanyAddress1).FontSize(7);

                        if (!string.IsNullOrEmpty(data.CompanyAddress2))
                            column.Item().AlignCenter().Text(data.CompanyAddress2).FontSize(7);

                        if (!string.IsNullOrEmpty(data.CompanyRtn))
                            column.Item().AlignCenter().Text($"RTN: {data.CompanyRtn}").FontSize(7);

                        if (!string.IsNullOrEmpty(data.CompanyPhone))
                            column.Item().AlignCenter().Text($"Tel: {data.CompanyPhone}").FontSize(7);

                        if (!string.IsNullOrEmpty(data.CompanyEmail))
                            column.Item().AlignCenter().Text(data.CompanyEmail).FontSize(7);

                        // Línea separadora
                        column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // === INFO FACTURA ===
                        column.Item().AlignCenter().Text($"FACTURA: {data.InvoiceNumber}")
                            .Bold().FontSize(9);
                        column.Item().AlignCenter().Text($"Fecha: {data.Date:dd/MM/yyyy HH:mm:ss}")
                            .FontSize(7);

                        // Línea separadora
                        column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // === INFO CLIENTE ===
                        column.Item().Text($"Cliente: {data.CustomerName}").FontSize(7);

                        if (!string.IsNullOrEmpty(data.CustomerRtn))
                            column.Item().Text($"RTN: {data.CustomerRtn}").FontSize(7);

                        if (!string.IsNullOrEmpty(data.CustomerAddress))
                            column.Item().Text($"Dir: {data.CustomerAddress}").FontSize(7);

                        // Línea separadora
                        column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // === TABLA DE PRODUCTOS ===
                        column.Item().Table(table =>
                        {
                            // Definir columnas
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(20); // ISV%
                                columns.RelativeColumn(3);  // Descripción
                                columns.ConstantColumn(20); // Cant
                                columns.ConstantColumn(35); // Total
                            });

                            // Header de la tabla
                            table.Header(header =>
                            {
                                header.Cell().Text("ISV").Bold().FontSize(6);
                                header.Cell().Text("Descripción").Bold().FontSize(6);
                                header.Cell().AlignRight().Text("Cant").Bold().FontSize(6);
                                header.Cell().AlignRight().Text("Total").Bold().FontSize(6);
                            });

                            // Filas de productos
                            foreach (var item in data.Items)
                            {
                                table.Cell().Text($"{item.TaxPercentage:0}%").FontSize(6);
                                table.Cell().Text(TruncateText(item.Description, 20)).FontSize(6);
                                table.Cell().AlignRight().Text(item.Quantity.ToString()).FontSize(6);
                                table.Cell().AlignRight().Text($"{item.TotalLine:N2}").FontSize(6);
                            }
                        });

                        // Línea separadora
                        column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // === DESGLOSE DE TOTALES ===
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text("Subtotal:").FontSize(7);
                            row.ConstantItem(50).AlignRight().Text($"{data.Subtotal:N2}").FontSize(7);
                        });

                        // Mostrar descuento solo si hay
                        if (data.TotalDiscounts > 0)
                        {
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().AlignRight().Text("Descuento:").FontSize(7).FontColor(Colors.Red.Medium);
                                row.ConstantItem(50).AlignRight().Text($"-{data.TotalDiscounts:N2}").FontSize(7).FontColor(Colors.Red.Medium);
                            });
                        }

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text("Exento:").FontSize(7);
                            row.ConstantItem(50).AlignRight().Text($"{data.Exempt:N2}").FontSize(7);
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text("Gravado 15%:").FontSize(7);
                            row.ConstantItem(50).AlignRight().Text($"{data.TaxedAt15Percent:N2}").FontSize(7);
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text("Gravado 18%:").FontSize(7);
                            row.ConstantItem(50).AlignRight().Text($"{data.TaxedAt18Percent:N2}").FontSize(7);
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text("ISV 15%:").FontSize(7);
                            row.ConstantItem(50).AlignRight().Text($"{data.TaxesAt15Percent:N2}").FontSize(7);
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text("ISV 18%:").FontSize(7);
                            row.ConstantItem(50).AlignRight().Text($"{data.TaxesAt18Percent:N2}").FontSize(7);
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().AlignRight().Text("TOTAL:").Bold().FontSize(8);
                            row.ConstantItem(50).AlignRight().Text($"{data.Total:N2}").Bold().FontSize(8);
                        });

                        // Línea separadora
                        column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // === VALOR EN LETRAS ===
                        string totalEnLetras = NumberToWordsConverter.ConvertToWords(data.Total);
                        column.Item().Text(totalEnLetras).FontSize(6).Italic();

                        // Línea separadora
                        column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // === PAGOS ===
                        foreach (var payment in data.Payments)
                        {
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"{payment.PaymentTypeName}:").FontSize(7);
                                row.ConstantItem(50).AlignRight().Text($"{payment.Amount:N2}").FontSize(7);
                            });
                        }

                        if (data.CashReceived.HasValue && data.CashReceived > 0)
                        {
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Efectivo Recibido:").FontSize(7);
                                row.ConstantItem(50).AlignRight().Text($"{data.CashReceived:N2}").FontSize(7);
                            });
                        }

                        if (data.ChangeGiven.HasValue && data.ChangeGiven > 0)
                        {
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Cambio:").FontSize(7);
                                row.ConstantItem(50).AlignRight().Text($"{data.ChangeGiven:N2}").FontSize(7);
                            });
                        }

                        // Línea separadora
                        column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // === FOOTER CAI ===
                        column.Item().AlignCenter().Text($"CAI: {data.CaiNumber}").FontSize(6);
                        column.Item().AlignCenter().Text($"Fecha Lím. Emisión: {data.CaiToDate:dd/MM/yyyy}").FontSize(6);
                        column.Item().AlignCenter().Text($"Rango Autorizado:").FontSize(6);
                        column.Item().AlignCenter().Text($"{data.InitialCorrelative} a {data.FinalCorrelative}").FontSize(6);

                        // Línea separadora
                        column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // === PIE DE PÁGINA ===
                        column.Item().AlignCenter().Text("Original: Cliente").FontSize(6);
                        column.Item().AlignCenter().Text("Copia: Obligado Tributario").FontSize(6);
                        column.Item().PaddingTop(5).AlignCenter().Text("¡Gracias por su compra!").Bold().FontSize(7);
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text ?? string.Empty;

            return text.Substring(0, maxLength - 3) + "...";
        }
    }
}
