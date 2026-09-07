namespace RestaurantFlow.Application.DTOs.Order;

public class CreateOrderDto
{
    public int TableId { get; set; }
    public string? Notes { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}
