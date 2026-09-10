using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Application.DTOs.Order;
using RestaurantFlow.Application.Interfaces;
using RestaurantFlow.Domain.Entities;
using RestaurantFlow.Domain.Enums;
using RestaurantFlow.Infrastructure.Data;

namespace RestaurantFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly AppDbContext _context;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, AppDbContext context, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _context = context;
        _logger = logger;
    }

    private int GetRestaurantId() => int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");
    private int GetUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

    /// <summary>
    /// Get all orders (can filter by status)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] string? status = null)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var orders = await _orderService.GetOrdersAsync(restaurantId);

            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status).ToList();
            }

            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener órdenes");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Get order by ID with details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var order = await _orderService.GetOrderByIdAsync(id, restaurantId);

            if (order == null)
                return NotFound(new { message = "Orden no encontrada" });

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener orden");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Get orders for a specific table
    /// </summary>
    [HttpGet("table/{tableId}")]
    public async Task<IActionResult> GetOrdersByTable(int tableId)
    {
        try
        {
            var restaurantId = GetRestaurantId();

            // Verify table belongs to restaurant
            var tableExists = await _context.Tables
                .AnyAsync(t => t.Id == tableId && t.RestaurantId == restaurantId);

            if (!tableExists)
                return NotFound(new { message = "Mesa no encontrada" });

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Waiter)
                .Where(o => o.RestaurantId == restaurantId && o.TableId == tableId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    TableId = o.TableId,
                    TableNumber = o.Table.Number,
                    WaiterId = o.WaiterId,
                    WaiterName = o.Waiter.Name,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status.ToString(),
                    Subtotal = o.Subtotal,
                    Tax = o.Tax,
                    Total = o.Total,
                    Notes = o.Notes,
                    CreatedAt = o.CreatedAt,
                    Items = o.OrderItems.Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Subtotal = i.Subtotal,
                        Notes = i.Notes
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener órdenes de mesa");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Create new order
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "WAITER,ADMIN")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        try
        {
            if (dto.Items == null || !dto.Items.Any())
                return BadRequest(new { message = "La orden debe tener al menos un item" });

            var restaurantId = GetRestaurantId();
            var waiterId = GetUserId();

            var order = await _orderService.CreateOrderAsync(dto, restaurantId, waiterId);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear orden");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        try
        {
            var restaurantId = GetRestaurantId();

            // Verify order exists
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.RestaurantId == restaurantId);

            if (order == null)
                return NotFound(new { message = "Orden no encontrada" });

            // Validate status
            if (!Enum.TryParse<OrderStatus>(dto.Status, out var newStatus))
                return BadRequest(new { message = "Estado de orden inválido" });

            // Check role permissions for status transitions
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            // KITCHEN can only mark as PREPARING or READY
            if (userRole == "KITCHEN" && newStatus != OrderStatus.PREPARING && newStatus != OrderStatus.READY)
                return Forbid();

            var result = await _orderService.UpdateOrderStatusAsync(id, dto.Status, restaurantId);

            if (!result)
                return NotFound(new { message = "Orden no encontrada" });

            return Ok(new { message = "Estado actualizado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estado de orden");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Add items to an existing order
    /// </summary>
    [HttpPost("{id}/items")]
    [Authorize(Roles = "WAITER,ADMIN")]
    public async Task<IActionResult> AddOrderItems(int id, [FromBody] List<CreateOrderItemDto> items)
    {
        try
        {
            if (items == null || !items.Any())
                return BadRequest(new { message = "Debe agregar al menos un item" });

            var restaurantId = GetRestaurantId();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == id && o.RestaurantId == restaurantId);

            if (order == null)
                return NotFound(new { message = "Orden no encontrada" });

            // Prevent adding items to completed or cancelled orders
            if (order.Status == OrderStatus.PAID || order.Status == OrderStatus.CANCELLED)
                return BadRequest(new { message = "No se pueden agregar items a una orden pagada o cancelada" });

            decimal subtotalIncrease = 0;

            foreach (var itemDto in items)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == itemDto.ProductId && p.RestaurantId == restaurantId);

                if (product == null)
                    return BadRequest(new { message = $"Producto {itemDto.ProductId} no encontrado" });

                if (!product.IsAvailable)
                    return BadRequest(new { message = $"Producto {product.Name} no está disponible" });

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price,
                    Subtotal = product.Price * itemDto.Quantity,
                    Notes = itemDto.Notes
                };

                _context.OrderItems.Add(orderItem);
                subtotalIncrease += orderItem.Subtotal;
            }

            // Update order totals
            order.Subtotal += subtotalIncrease;
            order.Tax = decimal.Round(order.Subtotal * 0.13m, 2);
            order.Total = order.Subtotal + order.Tax;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            var updatedOrder = await _orderService.GetOrderByIdAsync(id, restaurantId);
            return Ok(updatedOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar items a la orden");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Remove item from order
    /// </summary>
    [HttpDelete("{id}/items/{itemId}")]
    [Authorize(Roles = "WAITER,ADMIN")]
    public async Task<IActionResult> RemoveOrderItem(int id, int itemId)
    {
        try
        {
            var restaurantId = GetRestaurantId();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.RestaurantId == restaurantId);

            if (order == null)
                return NotFound(new { message = "Orden no encontrada" });

            // Prevent removing items from completed or cancelled orders
            if (order.Status == OrderStatus.PAID || order.Status == OrderStatus.CANCELLED)
                return BadRequest(new { message = "No se pueden remover items de una orden pagada o cancelada" });

            var orderItem = order.OrderItems.FirstOrDefault(i => i.Id == itemId);
            if (orderItem == null)
                return NotFound(new { message = "Item de orden no encontrado" });

            // Update order totals before removing item
            order.Subtotal -= orderItem.Subtotal;
            order.Tax = decimal.Round(order.Subtotal * 0.13m, 2);
            order.Total = order.Subtotal + order.Tax;

            _context.OrderItems.Remove(orderItem);
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            var updatedOrder = await _orderService.GetOrderByIdAsync(id, restaurantId);
            return Ok(updatedOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al remover item de la orden");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
