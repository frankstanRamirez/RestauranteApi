using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Application.DTOs.Order;
using RestaurantFlow.Application.Interfaces;
using RestaurantFlow.Domain.Entities;
using RestaurantFlow.Domain.Enums;

namespace RestaurantFlow.Application.Services;

public class OrderService : IOrderService
{
    private readonly IAppDbContext _context;
    private readonly INotificationService _notificationService;

    public OrderService(IAppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<List<OrderDto>> GetOrdersAsync(int restaurantId)
    {
        var orders = await _context.Orders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.RestaurantId == restaurantId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id, int restaurantId)
    {
        var order = await _context.Orders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.RestaurantId == restaurantId);

        return order == null ? null : MapToDto(order);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, int restaurantId, int waiterId)
    {
        // Validar mesa
        var table = await _context.Tables
            .FirstOrDefaultAsync(t => t.Id == dto.TableId && t.RestaurantId == restaurantId);
        
        if (table == null)
            throw new Exception("Mesa no encontrada");

        // Validar productos
        var productIds = dto.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && p.RestaurantId == restaurantId)
            .ToDictionaryAsync(p => p.Id);

        if (products.Count != productIds.Count)
            throw new Exception("Uno o más productos no existen");

        // Validar disponibilidad
        foreach (var item in dto.Items)
        {
            if (!products[item.ProductId].IsAvailable)
                throw new Exception($"El producto {products[item.ProductId].Name} no está disponible");
            
            if (item.Quantity <= 0)
                throw new Exception("La cantidad debe ser mayor a 0");
        }

        // Crear orden
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}";
        
        var order = new Order
        {
            RestaurantId = restaurantId,
            TableId = dto.TableId,
            WaiterId = waiterId,
            OrderNumber = orderNumber,
            Status = OrderStatus.PENDING,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        decimal subtotal = 0;
        foreach (var itemDto in dto.Items)
        {
            var product = products[itemDto.ProductId];
            var itemSubtotal = product.Price * itemDto.Quantity;
            subtotal += itemSubtotal;

            var orderItem = new OrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price,
                Subtotal = itemSubtotal,
                Notes = itemDto.Notes
            };
            order.OrderItems.Add(orderItem);
        }

        order.Subtotal = subtotal;
        order.Tax = subtotal * 0.13m; // 13% IVA
        order.Total = order.Subtotal + order.Tax;

        // Actualizar estado de mesa
        table.Status = TableStatus.OCCUPIED;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(order.Id, restaurantId) ?? throw new Exception("Error al crear orden");
    }

    public async Task<bool> UpdateOrderStatusAsync(int id, string status, int restaurantId)
    {
        var order = await _context.Orders
            .Include(o => o.Table)
            .FirstOrDefaultAsync(o => o.Id == id && o.RestaurantId == restaurantId);

        if (order == null)
            return false;

        if (!Enum.TryParse<OrderStatus>(status, out var newStatus))
            return false;

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<OrderDto>> GetKitchenOrdersAsync(int restaurantId)
    {
        var orders = await _context.Orders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.RestaurantId == restaurantId && 
                       (o.Status == OrderStatus.SENT_TO_KITCHEN || 
                        o.Status == OrderStatus.PREPARING || 
                        o.Status == OrderStatus.READY))
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            TableId = order.TableId,
            TableNumber = order.Table.Number,
            WaiterId = order.WaiterId,
            WaiterName = order.Waiter.Name,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            Subtotal = order.Subtotal,
            Tax = order.Tax,
            Total = order.Total,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Subtotal = oi.Subtotal,
                Notes = oi.Notes
            }).ToList()
        };
    }
}
