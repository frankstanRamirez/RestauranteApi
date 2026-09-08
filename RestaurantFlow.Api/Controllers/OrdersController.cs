using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantFlow.Application.DTOs.Order;
using RestaurantFlow.Application.Interfaces;

namespace RestaurantFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    private int GetRestaurantId() => int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");
    private int GetUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

    /// <summary>
    /// Get all orders
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var orders = await _orderService.GetOrdersAsync(restaurantId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener órdenes");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Get order by ID
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
    /// Create new order
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "WAITER,ADMIN")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        try
        {
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
}
