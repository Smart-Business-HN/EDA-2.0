namespace EDA.DOMAIN.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? RTN { get; set; }
        public string? Company { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Description { get; set; }
    }
}
