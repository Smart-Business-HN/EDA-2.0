namespace EDA.DOMAIN.Entities
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Owner { get; set; } = null!;
        public string? RTN { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber1 { get; set; }
        public string? PhoneNumber2 { get; set; }
        public string? Description { get; set; }
        public byte[]? Logo { get; set; }
    }
}
