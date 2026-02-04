using System;

namespace EDA.APPLICATION.DTOs
{
    public class TaxSummaryReportData
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalTaxedAt15 { get; set; }
        public decimal TotalTaxesAt15 { get; set; }
        public decimal TotalTaxedAt18 { get; set; }
        public decimal TotalTaxesAt18 { get; set; }
        public decimal TotalExempt { get; set; }
        public decimal GrandTotalTaxes { get; set; }
        public decimal GrandTotalSales { get; set; }
        public int TotalInvoices { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? CompanyAddress { get; set; }
        public string? CompanyRTN { get; set; }
    }
}
