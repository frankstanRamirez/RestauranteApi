namespace RestaurantFlow.Domain.Enums;

public enum OrderStatus
{
    PENDING,
    SENT_TO_KITCHEN,
    PREPARING,
    READY,
    DELIVERED,
    WAITING_PAYMENT,
    PAID,
    CANCELLED
}
