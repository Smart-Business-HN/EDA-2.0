using EDA.APPLICATION.DTOs;
using EDA.APPLICATION.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EDA.INFRAESTRUCTURE.Services
{
    public class ReportPdfService : IReportPdfService
    {
        static ReportPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateSalesByPeriodReportPdf(SalesByPeriodReportData data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => CreateHeader(c, data.CompanyName, data.CompanyRTN,
                        "REPORTE DE VENTAS POR PERIODO",
                        $"Del {data.FromDate:dd/MM/yyyy} al {data.ToDate.AddDays(-1):dd/MM/yyyy} | Agrupado por: {GetGroupingLabel(data.GroupingType)}"));

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(10);

                        // Summary cards
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Total Facturas", data.TotalInvoices.ToString()));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Total Ventas", $"L {data.GrandTotal:N2}"));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Total Impuestos", $"L {data.TotalTaxes:N2}"));
                        });

                        // Table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellHeaderStyle).Text("Periodo");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Facturas");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Subtotal");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Impuestos");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Total");
                            });

                            foreach (var item in data.Items)
                            {
                                table.Cell().Element(CellStyle).Text(item.PeriodLabel);
                                table.Cell().Element(CellStyle).AlignRight().Text(item.InvoiceCount.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text($"L {item.Subtotal:N2}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"L {item.TotalTaxes:N2}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"L {item.Total:N2}");
                            }

                            // Total row
                            table.Cell().Element(CellTotalStyle).Text("TOTAL");
                            table.Cell().Element(CellTotalStyle).AlignRight().Text(data.TotalInvoices.ToString());
                            table.Cell().Element(CellTotalStyle).AlignRight().Text($"L {data.Items.Sum(i => i.Subtotal):N2}");
                            table.Cell().Element(CellTotalStyle).AlignRight().Text($"L {data.TotalTaxes:N2}");
                            table.Cell().Element(CellTotalStyle).AlignRight().Text($"L {data.GrandTotal:N2}");
                        });
                    });

                    page.Footer().Element(CreateFooter);
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GeneratePaymentMethodsReportPdf(PaymentMethodsReportData data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => CreateHeader(c, data.CompanyName, data.CompanyRTN,
                        "REPORTE DE METODOS DE PAGO",
                        $"Del {data.FromDate:dd/MM/yyyy} al {data.ToDate.AddDays(-1):dd/MM/yyyy}"));

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Total Transacciones", data.TotalTransactions.ToString()));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Total Recaudado", $"L {data.GrandTotal:N2}"));
                        });

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellHeaderStyle).Text("Metodo de Pago");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Transacciones");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Monto Total");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Porcentaje");
                            });

                            foreach (var item in data.Items)
                            {
                                table.Cell().Element(CellStyle).Text(item.PaymentTypeName);
                                table.Cell().Element(CellStyle).AlignRight().Text(item.TransactionCount.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text($"L {item.TotalAmount:N2}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{item.Percentage:N1}%");
                            }

                            table.Cell().Element(CellTotalStyle).Text("TOTAL");
                            table.Cell().Element(CellTotalStyle).AlignRight().Text(data.TotalTransactions.ToString());
                            table.Cell().Element(CellTotalStyle).AlignRight().Text($"L {data.GrandTotal:N2}");
                            table.Cell().Element(CellTotalStyle).AlignRight().Text("100%");
                        });
                    });

                    page.Footer().Element(CreateFooter);
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateTaxSummaryReportPdf(TaxSummaryReportData data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => CreateHeader(c, data.CompanyName, data.CompanyRTN,
                        "RESUMEN DE IMPUESTOS",
                        $"Del {data.FromDate:dd/MM/yyyy} al {data.ToDate.AddDays(-1):dd/MM/yyyy}"));

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Total Facturas", data.TotalInvoices.ToString()));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Total Ventas", $"L {data.GrandTotalSales:N2}"));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Total Impuestos", $"L {data.GrandTotalTaxes:N2}"));
                        });

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellHeaderStyle).Text("Categoria");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Base Gravable");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Impuesto");
                            });

                            table.Cell().Element(CellStyle).Text("Gravado al 15%");
                            table.Cell().Element(CellStyle).AlignRight().Text($"L {data.TotalTaxedAt15:N2}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"L {data.TotalTaxesAt15:N2}");

                            table.Cell().Element(CellStyle).Text("Gravado al 18%");
                            table.Cell().Element(CellStyle).AlignRight().Text($"L {data.TotalTaxedAt18:N2}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"L {data.TotalTaxesAt18:N2}");

                            table.Cell().Element(CellStyle).Text("Exento");
                            table.Cell().Element(CellStyle).AlignRight().Text($"L {data.TotalExempt:N2}");
                            table.Cell().Element(CellStyle).AlignRight().Text("L 0.00");

                            table.Cell().Element(CellTotalStyle).Text("TOTAL");
                            table.Cell().Element(CellTotalStyle).AlignRight().Text($"L {data.TotalTaxedAt15 + data.TotalTaxedAt18 + data.TotalExempt:N2}");
                            table.Cell().Element(CellTotalStyle).AlignRight().Text($"L {data.GrandTotalTaxes:N2}");
                        });
                    });

                    page.Footer().Element(CreateFooter);
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateLowStockReportPdf(LowStockReportData data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => CreateHeader(c, data.CompanyName, data.CompanyRTN,
                        "REPORTE DE STOCK BAJO",
                        $"Generado: {data.GeneratedAt:dd/MM/yyyy HH:mm}"));

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Productos en Riesgo", data.TotalProductsAtRisk.ToString()));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Sin Stock", data.TotalOutOfStock.ToString(), Colors.Red.Medium));
                        });

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellHeaderStyle).Text("Producto");
                                header.Cell().Element(CellHeaderStyle).Text("Familia");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Stock");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Min");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Pedido Sug.");
                                header.Cell().Element(CellHeaderStyle).AlignCenter().Text("Estado");
                            });

                            foreach (var item in data.Products)
                            {
                                table.Cell().Element(CellStyle).Text(item.ProductName);
                                table.Cell().Element(CellStyle).Text(item.FamilyName);
                                table.Cell().Element(CellStyle).AlignRight().Text(item.CurrentStock.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text(item.MinStock.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text(item.SuggestedOrder.ToString());
                                table.Cell().Element(c => CellStatusStyle(c, item.Status)).AlignCenter().Text(item.Status);
                            }
                        });
                    });

                    page.Footer().Element(CreateFooter);
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateExpiringProductsReportPdf(ExpiringProductsReportData data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => CreateHeader(c, data.CompanyName, data.CompanyRTN,
                        "REPORTE DE PRODUCTOS POR VENCER",
                        $"Umbral: {data.DaysThreshold} dias | Generado: {data.GeneratedAt:dd/MM/yyyy HH:mm}"));

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Productos Vencidos", data.TotalExpired.ToString(), Colors.Red.Medium));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Por Vencer", data.TotalExpiring.ToString(), Colors.Orange.Medium));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Valor en Riesgo", $"L {data.TotalValueAtRisk:N2}"));
                        });

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellHeaderStyle).Text("Producto");
                                header.Cell().Element(CellHeaderStyle).Text("Familia");
                                header.Cell().Element(CellHeaderStyle).AlignCenter().Text("Vencimiento");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Dias");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Stock");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Valor Total");
                                header.Cell().Element(CellHeaderStyle).AlignCenter().Text("Estado");
                            });

                            foreach (var item in data.Products)
                            {
                                table.Cell().Element(CellStyle).Text(item.ProductName);
                                table.Cell().Element(CellStyle).Text(item.FamilyName);
                                table.Cell().Element(CellStyle).AlignCenter().Text(item.ExpirationDate?.ToString("dd/MM/yyyy") ?? "-");
                                table.Cell().Element(CellStyle).AlignRight().Text(item.DaysUntilExpiration.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text(item.CurrentStock.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text($"L {item.TotalValue:N2}");
                                table.Cell().Element(c => CellStatusStyle(c, item.Status)).AlignCenter().Text(item.Status);
                            }
                        });
                    });

                    page.Footer().Element(CreateFooter);
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateTopProductsReportPdf(TopProductsReportData data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => CreateHeader(c, data.CompanyName, data.CompanyRTN,
                        $"TOP {data.TopN} PRODUCTOS MAS VENDIDOS",
                        $"Del {data.FromDate:dd/MM/yyyy} al {data.ToDate.AddDays(-1):dd/MM/yyyy} | Ordenado por: {(data.SortBy == "Quantity" ? "Cantidad" : "Ingresos")}"));

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Total Unidades", data.TotalQuantity.ToString()));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Total Ingresos", $"L {data.TotalRevenue:N2}"));
                        });

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellHeaderStyle).AlignCenter().Text("#");
                                header.Cell().Element(CellHeaderStyle).Text("Producto");
                                header.Cell().Element(CellHeaderStyle).Text("Familia");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Cantidad");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Ingresos");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("% Total");
                            });

                            foreach (var item in data.Products)
                            {
                                table.Cell().Element(CellStyle).AlignCenter().Text(item.Rank.ToString());
                                table.Cell().Element(CellStyle).Text(item.ProductName);
                                table.Cell().Element(CellStyle).Text(item.FamilyName);
                                table.Cell().Element(CellStyle).AlignRight().Text(item.QuantitySold.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text($"L {item.TotalRevenue:N2}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"{item.PercentageOfTotal:N1}%");
                            }
                        });
                    });

                    page.Footer().Element(CreateFooter);
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateInventoryReportPdf(InventoryReportData data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => CreateHeader(c, data.CompanyName, data.CompanyRTN,
                        "REPORTE DE INVENTARIO",
                        $"Generado: {data.GeneratedAt:dd/MM/yyyy HH:mm}"));

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(10);

                        // Summary cards
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Total Productos", data.TotalProducts.ToString()));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Total Unidades", data.TotalUnits.ToString()));
                            row.ConstantItem(10);
                            row.RelativeItem().Element(c => CreateSummaryCard(c, "Valor Total", $"L {data.TotalInventoryValue:N2}"));
                        });

                        // Group tables by family
                        foreach (var familyGroup in data.FamilyGroups)
                        {
                            // Family header
                            column.Item().PaddingTop(10).Row(row =>
                            {
                                row.RelativeItem().Text(familyGroup.FamilyName).Bold().FontSize(11);
                                row.ConstantItem(150).AlignRight().Text($"Subtotal: L {familyGroup.TotalValue:N2}").FontSize(9).FontColor(Colors.Grey.Darken1);
                            });

                            // Products table
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1.2f);
                                    columns.RelativeColumn(1.5f);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellHeaderStyle).Text("Producto");
                                    header.Cell().Element(CellHeaderStyle).Text("Codigo");
                                    header.Cell().Element(CellHeaderStyle).AlignRight().Text("Stock");
                                    header.Cell().Element(CellHeaderStyle).AlignRight().Text("Precio");
                                    header.Cell().Element(CellHeaderStyle).AlignRight().Text("Valor Total");
                                });

                                foreach (var product in familyGroup.Products)
                                {
                                    table.Cell().Element(CellStyle).Text(product.ProductName);
                                    table.Cell().Element(CellStyle).Text(product.Barcode ?? "-");
                                    table.Cell().Element(CellStyle).AlignRight().Text(product.Stock.ToString());
                                    table.Cell().Element(CellStyle).AlignRight().Text($"L {product.Price:N2}");
                                    table.Cell().Element(CellStyle).AlignRight().Text($"L {product.TotalValue:N2}");
                                }

                                // Subtotal row for family
                                table.Cell().Element(CellTotalStyle).Text($"Subtotal {familyGroup.FamilyName}");
                                table.Cell().Element(CellTotalStyle).Text("");
                                table.Cell().Element(CellTotalStyle).AlignRight().Text(familyGroup.TotalUnits.ToString());
                                table.Cell().Element(CellTotalStyle).Text("");
                                table.Cell().Element(CellTotalStyle).AlignRight().Text($"L {familyGroup.TotalValue:N2}");
                            });
                        }

                        // Grand total
                        column.Item().PaddingTop(15).Background(Colors.Grey.Lighten3).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Text("GRAN TOTAL").Bold().FontSize(12);
                            row.ConstantItem(100).AlignRight().Text($"{data.TotalUnits} uds").Bold();
                            row.ConstantItem(150).AlignRight().Text($"L {data.TotalInventoryValue:N2}").Bold().FontSize(12);
                        });
                    });

                    page.Footer().Element(CreateFooter);
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateMonthlyClosingReportPdf(MonthlyClosingReportData data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => CreateHeader(c, data.CompanyName, data.CompanyRTN,
                        "CIERRE DE MES",
                        $"{data.MonthName} {data.Year} | Generado: {data.GeneratedAt:dd/MM/yyyy HH:mm}"));

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(15);

                        // Period info
                        column.Item().Background(Colors.Blue.Lighten5).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Text($"Periodo: {data.PeriodStart:dd/MM/yyyy} - {data.PeriodEnd:dd/MM/yyyy}").FontSize(11);
                            row.ConstantItem(150).AlignRight().Text($"Facturas: {data.TotalInvoices}").Bold();
                        });

                        // Sales summary
                        column.Item().Text("RESUMEN DE VENTAS").Bold().FontSize(12);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Cell().Element(CellStyle).Text("Subtotal Ventas");
                            table.Cell().Element(CellStyle).AlignRight().Text($"L {data.TotalSubtotal:N2}");

                            table.Cell().Element(CellStyle).Text("Total Descuentos");
                            table.Cell().Element(CellStyle).AlignRight().Text($"L {data.TotalDiscounts:N2}");

                            table.Cell().Element(CellStyle).Text("Total Impuestos");
                            table.Cell().Element(CellStyle).AlignRight().Text($"L {data.GrandTotalTaxes:N2}");

                            table.Cell().Element(CellTotalStyle).Text("TOTAL VENTAS");
                            table.Cell().Element(CellTotalStyle).AlignRight().Text($"L {data.TotalSales:N2}");
                        });

                        // Tax breakdown
                        column.Item().PaddingTop(10).Text("DESGLOSE DE IMPUESTOS").Bold().FontSize(12);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellHeaderStyle).Text("Categoria");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Base Gravable");
                                header.Cell().Element(CellHeaderStyle).AlignRight().Text("Impuesto");
                            });

                            table.Cell().Element(CellStyle).Text("Gravado al 15%");
                            table.Cell().Element(CellStyle).AlignRight().Text($"L {data.TotalTaxedAt15:N2}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"L {data.TotalTaxesAt15:N2}");

                            table.Cell().Element(CellStyle).Text("Gravado al 18%");
                            table.Cell().Element(CellStyle).AlignRight().Text($"L {data.TotalTaxedAt18:N2}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"L {data.TotalTaxesAt18:N2}");

                            table.Cell().Element(CellStyle).Text("Exento");
                            table.Cell().Element(CellStyle).AlignRight().Text($"L {data.TotalExempt:N2}");
                            table.Cell().Element(CellStyle).AlignRight().Text("L 0.00");

                            table.Cell().Element(CellTotalStyle).Text("TOTAL");
                            table.Cell().Element(CellTotalStyle).AlignRight().Text($"L {data.TotalTaxedAt15 + data.TotalTaxedAt18 + data.TotalExempt:N2}");
                            table.Cell().Element(CellTotalStyle).AlignRight().Text($"L {data.GrandTotalTaxes:N2}");
                        });

                        // Grand summary
                        column.Item().PaddingTop(20).Background(Colors.Grey.Lighten3).Padding(15).Column(c =>
                        {
                            c.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Total Facturas:").FontSize(11);
                                row.ConstantItem(150).AlignRight().Text(data.TotalInvoices.ToString()).Bold().FontSize(11);
                            });
                            c.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("Total Impuestos Recaudados:").FontSize(11);
                                row.ConstantItem(150).AlignRight().Text($"L {data.GrandTotalTaxes:N2}").Bold().FontSize(11);
                            });
                            c.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL VENTAS DEL MES:").Bold().FontSize(14);
                                row.ConstantItem(150).AlignRight().Text($"L {data.TotalSales:N2}").Bold().FontSize(14);
                            });
                        });

                        // Signature line
                        column.Item().PaddingTop(40).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                                c.Item().PaddingTop(5).AlignCenter().Text("Elaborado por").FontSize(9);
                            });
                            row.ConstantItem(50);
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                                c.Item().PaddingTop(5).AlignCenter().Text("Autorizado por").FontSize(9);
                            });
                        });
                    });

                    page.Footer().Element(CreateFooter);
                });
            });

            return document.GeneratePdf();
        }

        // Helper methods
        private static void CreateHeader(IContainer container, string companyName, string? companyRtn, string title, string subtitle)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text(companyName).Bold().FontSize(14);
                        if (!string.IsNullOrEmpty(companyRtn))
                            c.Item().Text($"RTN: {companyRtn}").FontSize(9);
                    });
                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().AlignRight().Text(title).Bold().FontSize(12);
                        c.Item().AlignRight().Text(subtitle).FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });
                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
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
            container.AlignCenter().Text(text =>
            {
                text.Span($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").FontSize(8).FontColor(Colors.Grey.Darken1);
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

        private static IContainer CellTotalStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.Bold().FontSize(9))
                .PaddingVertical(5)
                .PaddingHorizontal(3)
                .Background(Colors.Grey.Lighten3)
                .BorderTop(1)
                .BorderColor(Colors.Grey.Medium);
        }

        private static IContainer CellStatusStyle(IContainer container, string status)
        {
            var bgColor = status switch
            {
                "Sin Stock" or "Vencido" => Colors.Red.Lighten4,
                "Critico" => Colors.Orange.Lighten4,
                _ => Colors.Yellow.Lighten4
            };

            return container.DefaultTextStyle(x => x.FontSize(8))
                .PaddingVertical(4)
                .PaddingHorizontal(3)
                .Background(bgColor)
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten1);
        }

        private static string GetGroupingLabel(string groupingType)
        {
            return groupingType switch
            {
                "Week" => "Semana",
                "Month" => "Mes",
                _ => "Dia"
            };
        }
    }
}
