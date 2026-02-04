using System;
using System.Collections.Generic;

namespace EDA.APPLICATION.DTOs
{
    public class LowStockReportData
    {
        public DateTime GeneratedAt { get; set; }
        public List<LowStockItem> Products { get; set; } = new();
        public int TotalProductsAtRisk { get; set; }
        public int TotalOutOfStock { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? CompanyAddress { get; set; }
        public string? CompanyRTN { get; set; }
    }

    public class LowStockItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Barcode { get; set; }
        public string FamilyName { get; set; } = null!;
        public int CurrentStock { get; set; }
        public int MinStock { get; set; }
        public int MaxStock { get; set; }
        public int SuggestedOrder { get; set; }
        public decimal UnitPrice { get; set; }
        public string Status { get; set; } = null!;
    }
}
