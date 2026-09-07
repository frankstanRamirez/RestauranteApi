namespace RestaurantFlow.Application.DTOs.Order;

public class OrderDto
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public int TableNumber { get; set; }
    public int WaiterId { get; set; }
    public string WaiterName { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
