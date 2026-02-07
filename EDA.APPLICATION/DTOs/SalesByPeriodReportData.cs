namespace EDA.APPLICATION.DTOs
{
    public class SalesByPeriodReportData
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string GroupingType { get; set; } = "Day";
        public List<SalesPeriodItem> Items { get; set; } = new();
        public decimal GrandTotal { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalTaxes { get; set; }
        public decimal TotalDiscounts { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? CompanyAddress { get; set; }
        public string? CompanyRTN { get; set; }
    }

    public class SalesPeriodItem
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string PeriodLabel { get; set; } = null!;
        public int InvoiceCount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalTaxes { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal Total { get; set; }
    }
}
