namespace RestaurantFlow.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Restaurant Restaurant { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
