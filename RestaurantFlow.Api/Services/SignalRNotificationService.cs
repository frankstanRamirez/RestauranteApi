using Microsoft.AspNetCore.SignalR;
using RestaurantFlow.Api.Hubs;
using RestaurantFlow.Application.Interfaces;

namespace RestaurantFlow.Api.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<RestaurantHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<RestaurantHub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyNewOrderAsync(int restaurantId, object orderData)
    {
        try
        {
            await _hubContext.Clients
                .Group($"Restaurant_{restaurantId}")
                .SendAsync("NewOrder", orderData);
            
            _logger.LogInformation("Notificación NewOrder enviada al restaurante {RestaurantId}", restaurantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación NewOrder");
        }
    }

    public async Task NotifyOrderStatusChangedAsync(int restaurantId, int orderId, string newStatus)
    {
        try
        {
            await _hubContext.Clients
                .Group($"Restaurant_{restaurantId}")
                .SendAsync("OrderStatusChanged", new { orderId, newStatus });
            
            _logger.LogInformation("Notificación OrderStatusChanged enviada: Order {OrderId} -> {Status}", orderId, newStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación OrderStatusChanged");
        }
    }

    public async Task NotifyTableStatusChangedAsync(int restaurantId, int tableId, string newStatus)
    {
        try
        {
            await _hubContext.Clients
                .Group($"Restaurant_{restaurantId}")
                .SendAsync("TableStatusChanged", new { tableId, newStatus });
            
            _logger.LogInformation("Notificación TableStatusChanged enviada: Table {TableId} -> {Status}", tableId, newStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación TableStatusChanged");
        }
    }

    public async Task NotifyReservationCreatedAsync(int restaurantId, object reservationData)
    {
        try
        {
            await _hubContext.Clients
                .Group($"Restaurant_{restaurantId}")
                .SendAsync("ReservationCreated", reservationData);
            
            _logger.LogInformation("Notificación ReservationCreated enviada al restaurante {RestaurantId}", restaurantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación ReservationCreated");
        }
    }

    public async Task NotifyReservationStatusChangedAsync(int restaurantId, int reservationId, string newStatus)
    {
        try
        {
            await _hubContext.Clients
                .Group($"Restaurant_{restaurantId}")
                .SendAsync("ReservationStatusChanged", new { reservationId, newStatus });
            
            _logger.LogInformation("Notificación ReservationStatusChanged enviada: Reservation {ReservationId} -> {Status}", reservationId, newStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación ReservationStatusChanged");
        }
    }
}
