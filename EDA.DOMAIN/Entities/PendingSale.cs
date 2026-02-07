namespace EDA.DOMAIN.Entities
{
    public class PendingSale
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = null!;
        public string JsonData { get; set; } = null!;
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
