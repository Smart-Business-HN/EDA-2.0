namespace EDA.DOMAIN.Entities
{
    public class CashRegister
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!; // Ej: "C001", "BAR01"
        public bool IsActive { get; set; } = true;
        public DateTime CreationDate { get; set; }
        public int PrinterConfigurationId { get; set; }
        public virtual PrinterConfiguration? PrinterConfiguration { get; set; }
    }
}
