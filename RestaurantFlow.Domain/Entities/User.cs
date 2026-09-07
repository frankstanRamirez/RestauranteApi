using RestaurantFlow.Domain.Enums;

namespace RestaurantFlow.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Restaurant Restaurant { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
