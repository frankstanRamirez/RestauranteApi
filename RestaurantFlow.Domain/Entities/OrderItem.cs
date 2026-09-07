namespace RestaurantFlow.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
