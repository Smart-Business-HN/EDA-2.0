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

        // Desglose de impuestos
        public decimal TaxedAt15Percent { get; set; }    // Base gravada al 15%
        public decimal TaxesAt15Percent { get; set; }    // Impuesto al 15%
        public decimal TaxedAt18Percent { get; set; }    // Base gravada al 18%
        public decimal TaxesAt18Percent { get; set; }    // Impuesto al 18%
        public decimal Exempt { get; set; }               // Total exento

        // Credit fields
        public int Status { get; set; }
        public decimal OutstandingAmount { get; set; }
        public int? CreditDays { get; set; }
        public DateTime? DueDate { get; set; }

        public List<SoldProduct>? SoldProducts { get; set; }
        public List<InvoicePayment>? InvoicePayments { get; set; }
    }
}
