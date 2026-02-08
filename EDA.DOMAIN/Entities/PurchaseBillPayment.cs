namespace EDA.DOMAIN.Entities
{
    public class PurchaseBillPayment
    {
        public int Id { get; init; }
        public int PurchaseBillId { get; set; }
        public PurchaseBill? PurchaseBill { get; set; }
        public int PaymentTypeId { get; set; }
        public PaymentType? PaymentType { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; } = null!;
    }
}
