namespace EDA.APPLICATION.DTOs
{
    public class PaymentMethodsReportData
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<PaymentMethodItem> Items { get; set; } = new();
        public decimal GrandTotal { get; set; }
        public int TotalTransactions { get; set; }
        public string CompanyName { get; set; } = null!;
        public string? CompanyAddress { get; set; }
        public string? CompanyRTN { get; set; }
    }

    public class PaymentMethodItem
    {
        public int PaymentTypeId { get; set; }
        public string PaymentTypeName { get; set; } = null!;
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }
}
