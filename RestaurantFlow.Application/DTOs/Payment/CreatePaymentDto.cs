namespace RestaurantFlow.Application.DTOs.Payment;

public class CreatePaymentDto
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Reference { get; set; }
}
