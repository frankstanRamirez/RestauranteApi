namespace RestaurantFlow.Application.DTOs.Table;

public class CreateTableDto
{
    public int Number { get; set; }
    public int Capacity { get; set; }
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string Shape { get; set; } = "SQUARE";
}
