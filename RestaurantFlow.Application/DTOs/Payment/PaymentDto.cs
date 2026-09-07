namespace RestaurantFlow.Application.DTOs.Payment;

public class PaymentDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public string? Reference { get; set; }
}
