using System;
using System.Collections.Generic;

namespace EDA.APPLICATION.DTOs
{
    public class TopProductsReportData
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TopN { get; set; }
        public string SortBy { get; set; } = "Revenue";
        public List<TopProductItem> Products { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public int TotalQuantity { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? CompanyAddress { get; set; }
        public string? CompanyRTN { get; set; }
    }

    public class TopProductItem
    {
        public int Rank { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Barcode { get; set; }
        public string FamilyName { get; set; } = null!;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal PercentageOfTotal { get; set; }
    }
}
