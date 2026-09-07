namespace RestaurantFlow.Application.DTOs.Table;

public class TableDto
{
    public int Id { get; set; }
    public int Number { get; set; }
    public int Capacity { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string Shape { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
