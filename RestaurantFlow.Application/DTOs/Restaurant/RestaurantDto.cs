namespace RestaurantFlow.Application.DTOs.Restaurant;

public class RestaurantDto
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
    public bool IsActive { get; set; }
}
