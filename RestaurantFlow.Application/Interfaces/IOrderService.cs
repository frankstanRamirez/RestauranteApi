using RestaurantFlow.Application.DTOs.Order;

namespace RestaurantFlow.Application.Interfaces;

public interface IOrderService
{
    Task<List<OrderDto>> GetOrdersAsync(int restaurantId);
    Task<OrderDto?> GetOrderByIdAsync(int id, int restaurantId);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, int restaurantId, int waiterId);
    Task<bool> UpdateOrderStatusAsync(int id, string status, int restaurantId);
    Task<List<OrderDto>> GetKitchenOrdersAsync(int restaurantId);
}
