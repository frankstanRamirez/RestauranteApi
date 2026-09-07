namespace RestaurantFlow.Domain.Entities;

public class Promotion
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Restaurant Restaurant { get; set; } = null!;
}
