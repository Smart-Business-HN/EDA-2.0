namespace EDA.APPLICATION.DTOs
{
    public class InventoryReportData
    {
        public DateTime GeneratedAt { get; set; }
        public List<InventoryFamilyGroup> FamilyGroups { get; set; } = new();
        public int TotalProducts { get; set; }
        public int TotalUnits { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? CompanyAddress { get; set; }
        public string? CompanyRTN { get; set; }
    }

    public class InventoryFamilyGroup
    {
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = null!;
        public List<InventoryProductItem> Products { get; set; } = new();
        public int TotalProducts { get; set; }
        public int TotalUnits { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class InventoryProductItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? Barcode { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public decimal TotalValue { get; set; }
    }
}
