using RestaurantFlow.Domain.Enums;

namespace RestaurantFlow.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public int TableId { get; set; }
    public int WaiterId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.PENDING;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Restaurant Restaurant { get; set; } = null!;
    public Table Table { get; set; } = null!;
    public User Waiter { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
}
