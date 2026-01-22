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

                        // === LOGO DE LA EMPRESA ===
                        if (data.CompanyLogo != null && data.CompanyLogo.Length > 0)
                        {
                            column.Item().AlignCenter().Width(60).Image(data.CompanyLogo);
                            column.Item().PaddingVertical(2);
                        }

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
                                table.Cell().Text(item.Description).FontSize(6);
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
                                row.RelativeItem().AlignRight().Text("Descuento:").FontSize(7);
                                row.ConstantItem(50).AlignRight().Text($"-{data.TotalDiscounts:N2}").FontSize(7);
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

                        // === CAMPOS FISCALES HONDURAS ===
                        column.Item().Text("N° Orden de Compra Exenta:________").FontSize(6);
                        column.Item().Text("Constancia Reg. Exonerados:________").FontSize(6);
                        column.Item().Text("Registro SAG:____________________").FontSize(6);

                        // Línea separadora
                        column.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Black);

                        // === PIE DE PÁGINA ===
                        column.Item().AlignCenter().Text("Original: Cliente").FontSize(6);
                        column.Item().AlignCenter().Text("Copia: Obligado Tributario").FontSize(6);
                        column.Item().PaddingTop(3).AlignCenter().Text("LA FACTURA ES BENEFICIO DE TODOS").Bold().FontSize(6);
                        column.Item().AlignCenter().Text("EXIJALA").Bold().FontSize(6);
                        column.Item().PaddingTop(5).AlignCenter().Text("¡Gracias por su compra!").Bold().FontSize(7);
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateInvoiceLetterPdf(InvoicePdfData data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Configurar página tamaño carta
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Inch);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(container => ComposeLetterHeader(container, data));
                    page.Content().Element(container => ComposeLetterContent(container, data));
                    page.Footer().Element(container => ComposeLetterFooter(container, data));
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeLetterHeader(IContainer container, InvoicePdfData data)
        {
            container.Column(column =>
            {
                // Encabezado de la empresa
                column.Item().Row(row =>
                {
                    // Logo de la empresa (si existe)
                    if (data.CompanyLogo != null && data.CompanyLogo.Length > 0)
                    {
                        row.ConstantItem(80).AlignMiddle().Image(data.CompanyLogo);
                        row.ConstantItem(15);
                    }

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(data.CompanyName).Bold().FontSize(16);

                        if (!string.IsNullOrEmpty(data.CompanyAddress1))
                            col.Item().Text(data.CompanyAddress1).FontSize(9);

                        if (!string.IsNullOrEmpty(data.CompanyAddress2))
                            col.Item().Text(data.CompanyAddress2).FontSize(9);

                        if (!string.IsNullOrEmpty(data.CompanyRtn))
                            col.Item().Text($"RTN: {data.CompanyRtn}").FontSize(9);

                        if (!string.IsNullOrEmpty(data.CompanyPhone))
                            col.Item().Text($"Tel: {data.CompanyPhone}").FontSize(9);

                        if (!string.IsNullOrEmpty(data.CompanyEmail))
                            col.Item().Text(data.CompanyEmail).FontSize(9);
                    });

                    row.ConstantItem(180).Column(col =>
                    {
                        col.Item().AlignRight().Text("FACTURA").Bold().FontSize(18);
                        col.Item().AlignRight().Text(data.InvoiceNumber).Bold().FontSize(12);
                        col.Item().AlignRight().Text($"Fecha: {data.Date:dd/MM/yyyy}").FontSize(10);
                        col.Item().AlignRight().Text($"Hora: {data.Date:HH:mm:ss}").FontSize(10);
                    });
                });

                column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
            });
        }

        private void ComposeLetterContent(IContainer container, InvoicePdfData data)
        {
            container.Column(column =>
            {
                // Info del cliente
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("CLIENTE").Bold().FontSize(11);
                        col.Item().Text(data.CustomerName).FontSize(10);

                        if (!string.IsNullOrEmpty(data.CustomerRtn))
                            col.Item().Text($"RTN: {data.CustomerRtn}").FontSize(10);

                        if (!string.IsNullOrEmpty(data.CustomerAddress))
                            col.Item().Text($"Dirección: {data.CustomerAddress}").FontSize(10);
                    });
                });

                column.Item().PaddingVertical(15);

                // Tabla de productos
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // ISV%
                        columns.RelativeColumn(4);   // Descripción
                        columns.ConstantColumn(50);  // Cantidad
                        columns.ConstantColumn(70);  // Precio Unit.
                        columns.ConstantColumn(80);  // Total
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                            .Text("ISV%").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                            .Text("Descripción").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                            .AlignRight().Text("Cantidad").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                            .AlignRight().Text("Precio Unit.").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                            .AlignRight().Text("Total").Bold().FontSize(9);
                    });

                    // Filas
                    foreach (var item in data.Items)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text($"{item.TaxPercentage:0}%").FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(item.Description).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .AlignRight().Text(item.Quantity.ToString()).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .AlignRight().Text($"L {item.UnitPrice:N2}").FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .AlignRight().Text($"L {item.TotalLine:N2}").FontSize(9);
                    }
                });

                column.Item().PaddingVertical(10);

                // Totales y pagos en dos columnas
                column.Item().Row(row =>
                {
                    // Columna izquierda: Pagos
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("PAGOS").Bold().FontSize(11);
                        col.Item().PaddingTop(5);

                        foreach (var payment in data.Payments)
                        {
                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"{payment.PaymentTypeName}:").FontSize(10);
                                r.ConstantItem(80).AlignRight().Text($"L {payment.Amount:N2}").FontSize(10);
                            });
                        }

                        if (data.CashReceived.HasValue && data.CashReceived > 0)
                        {
                            col.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text("Efectivo Recibido:").FontSize(10);
                                r.ConstantItem(80).AlignRight().Text($"L {data.CashReceived:N2}").FontSize(10);
                            });
                        }

                        if (data.ChangeGiven.HasValue && data.ChangeGiven > 0)
                        {
                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Cambio:").FontSize(10);
                                r.ConstantItem(80).AlignRight().Text($"L {data.ChangeGiven:N2}").FontSize(10);
                            });
                        }
                    });

                    row.ConstantItem(30);

                    // Columna derecha: Totales
                    row.ConstantItem(220).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
                    {
                        void AddTotalRow(string label, decimal value, bool isBold = false)
                        {
                            col.Item().Row(r =>
                            {
                                var labelText = r.RelativeItem().Text(label).FontSize(10);
                                var valueText = r.ConstantItem(80).AlignRight().Text($"L {value:N2}").FontSize(10);

                                if (isBold)
                                {
                                    labelText.Bold();
                                    valueText.Bold();
                                }
                            });
                        }

                        AddTotalRow("Subtotal:", data.Subtotal);

                        if (data.TotalDiscounts > 0)
                            AddTotalRow("Descuento:", (decimal)data.TotalDiscounts * -1);

                        AddTotalRow("Exento:", data.Exempt);
                        AddTotalRow("Gravado 15%:", data.TaxedAt15Percent);
                        AddTotalRow("ISV 15%:", data.TaxesAt15Percent);
                        AddTotalRow("Gravado 18%:", data.TaxedAt18Percent);
                        AddTotalRow("ISV 18%:", data.TaxesAt18Percent);

                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL:").Bold().FontSize(12);
                            r.ConstantItem(80).AlignRight().Text($"L {data.Total:N2}").Bold().FontSize(12);
                        });
                    });
                });

                column.Item().PaddingVertical(10);

                // Valor en letras
                column.Item().Background(Colors.Grey.Lighten4).Padding(8).Column(col =>
                {
                    col.Item().Text("SON:").Bold().FontSize(9);
                    string totalEnLetras = NumberToWordsConverter.ConvertToWords(data.Total);
                    col.Item().Text(totalEnLetras).Italic().FontSize(9);
                });
            });
        }

        private void ComposeLetterFooter(IContainer container, InvoicePdfData data)
        {
            container.Column(column =>
            {
                column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("INFORMACIÓN FISCAL").Bold().FontSize(9);
                        col.Item().Text($"CAI: {data.CaiNumber}").FontSize(8);
                        col.Item().Text($"Fecha Límite de Emisión: {data.CaiToDate:dd/MM/yyyy}").FontSize(8);
                        col.Item().Text($"Rango Autorizado: {data.InitialCorrelative} a {data.FinalCorrelative}").FontSize(8);
                    });

                    row.ConstantItem(200).Column(col =>
                    {
                        col.Item().AlignRight().Text("Original: Cliente").FontSize(8);
                        col.Item().AlignRight().Text("Copia: Obligado Tributario").FontSize(8);
                    });
                });

                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("N° Orden de Compra Exenta:____________________________").FontSize(8);
                        col.Item().Text("Constancia Reg. Exonerados:____________________________").FontSize(8);
                        col.Item().Text("Registro SAG:__________________________________________").FontSize(8);
                    });

                    row.ConstantItem(200).Column(col =>
                    {
                        col.Item().AlignCenter().Text("LA FACTURA ES BENEFICIO DE TODOS").Bold().FontSize(8);
                        col.Item().AlignCenter().Text("EXIJALA").Bold().FontSize(8);
                    });
                });
            });
        }

    }
}
