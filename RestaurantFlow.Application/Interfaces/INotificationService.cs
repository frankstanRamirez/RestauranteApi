namespace RestaurantFlow.Application.Interfaces;

public interface INotificationService
{
    Task NotifyNewOrderAsync(int restaurantId, object orderData);
    Task NotifyOrderStatusChangedAsync(int restaurantId, int orderId, string newStatus);
    Task NotifyTableStatusChangedAsync(int restaurantId, int tableId, string newStatus);
    Task NotifyReservationCreatedAsync(int restaurantId, object reservationData);
    Task NotifyReservationStatusChangedAsync(int restaurantId, int reservationId, string newStatus);
}
