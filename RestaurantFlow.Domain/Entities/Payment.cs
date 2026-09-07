using RestaurantFlow.Domain.Enums;

namespace RestaurantFlow.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public int CashierId { get; set; }
    public string? Reference { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
    public User Cashier { get; set; } = null!;
}
