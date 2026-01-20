namespace EDA.DOMAIN.Entities
{
    public class Invoice
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public int CaiId { get; set; }
        public Cai? Cai { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public double? CashReceived { get; set; }
        public double? ChangeGiven { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int? DiscountId { get; set; }
        public Discount? Discount { get; set; }
        public decimal Subtotal { get; set; }
        public double TotalDiscounts { get; set; }
        public double TotalTaxes { get; set; }
        public decimal Total { get; set; }
        public List<SoldProduct>? SoldProducts { get; set; }
        public List<InvoicePayment>? InvoicePayments { get; set; }
    }
}
