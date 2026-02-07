namespace EDA.APPLICATION.DTOs
{
    public class MonthlyClosingReportData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = null!;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime GeneratedAt { get; set; }

        // Totales de ventas
        public int TotalInvoices { get; set; }
        public decimal TotalSubtotal { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal TotalSales { get; set; }

        // Desglose de impuestos
        public decimal TotalTaxedAt15 { get; set; }
        public decimal TotalTaxesAt15 { get; set; }
        public decimal TotalTaxedAt18 { get; set; }
        public decimal TotalTaxesAt18 { get; set; }
        public decimal TotalExempt { get; set; }
        public decimal GrandTotalTaxes { get; set; }

        // Datos de empresa
        public string CompanyName { get; set; } = null!;
        public string? CompanyAddress { get; set; }
        public string? CompanyRTN { get; set; }
    }
}
