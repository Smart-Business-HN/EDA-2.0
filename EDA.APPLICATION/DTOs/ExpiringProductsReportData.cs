namespace EDA.APPLICATION.DTOs
{
    public class ExpiringProductsReportData
    {
        public DateTime GeneratedAt { get; set; }
        public int DaysThreshold { get; set; }
        public List<ExpiringProductItem> Products { get; set; } = new();
        public int TotalExpired { get; set; }
        public int TotalExpiring { get; set; }
        public decimal TotalValueAtRisk { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? CompanyAddress { get; set; }
        public string? CompanyRTN { get; set; }
    }

    public class ExpiringProductItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Barcode { get; set; }
        public string FamilyName { get; set; } = null!;
        public DateTime? ExpirationDate { get; set; }
        public int DaysUntilExpiration { get; set; }
        public int CurrentStock { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
        public string Status { get; set; } = null!;
    }
}
