namespace EDA.DOMAIN.Entities
{
    public class Tax
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Percentage { get; set; }
    }
}
