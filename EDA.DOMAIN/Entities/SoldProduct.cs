using System;
using System.Collections.Generic;
using System.Text;

namespace EDA.DOMAIN.Entities
{
    public class SoldProduct
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public int TaxId { get; set; }
        public Tax? Tax { get; set; }
        public double? TaxAmount { get; set; } = 0;
        public int? DiscountId { get; set; }
        public Discount? Discount { get; set; }
        public double? DiscountAmount { get; set; } = 0;
        public decimal UnitPrice { get; set; }
        public decimal TotalLine { get; set; }
    }
}
