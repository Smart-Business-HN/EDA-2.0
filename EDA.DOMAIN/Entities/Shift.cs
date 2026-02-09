namespace EDA.DOMAIN.Entities
{
    public class Shift
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string ShiftType { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal InitialAmount { get; set; }
        public decimal? FinalCashAmount { get; set; }
        public decimal? FinalCardAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public decimal? ExpectedAmount { get; set; }
        public decimal? Difference { get; set; }
        public bool IsOpen { get; set; }

        // Caja Registradora
        public int? CashRegisterId { get; set; }
        public virtual CashRegister? CashRegister { get; set; }
    }
}
