namespace EDA.APPLICATION.DTOs
{
    public class SalesSummaryData
    {
        public List<SalesSummaryUserItem> UserSummaries { get; set; } = new();
        public List<SalesSummaryShiftItem> ShiftDetails { get; set; } = new();
        public int GrandTotalShifts { get; set; }
        public int GrandTotalInvoices { get; set; }
        public decimal GrandTotalSales { get; set; }
    }

    public class SalesSummaryUserItem
    {
        public string UserName { get; set; } = null!;
        public int TotalShifts { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalSales { get; set; }
    }

    public class SalesSummaryShiftItem
    {
        public string UserName { get; set; } = null!;
        public string ShiftType { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal InitialAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public decimal? Difference { get; set; }
        public int InvoiceCount { get; set; }
        public decimal TotalSales { get; set; }
    }
}
