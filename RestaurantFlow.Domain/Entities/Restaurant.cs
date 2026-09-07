namespace RestaurantFlow.Domain.Entities;

public class Restaurant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public TimeSpan? OpeningTime { get; set; }
    public TimeSpan? ClosingTime { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Table> Tables { get; set; } = new List<Table>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}
