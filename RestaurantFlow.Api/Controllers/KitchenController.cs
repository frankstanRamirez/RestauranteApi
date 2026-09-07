using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantFlow.Application.Interfaces;

namespace RestaurantFlow.Api.Controllers;

[Authorize(Roles = "KITCHEN,ADMIN")]
[ApiController]
[Route("api/[controller]")]
public class KitchenController : ControllerBase
{
    private readonly IOrderService _orderService;

    public KitchenController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private int GetRestaurantId() => int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");

    /// <summary>
    /// Obtener órdenes de cocina
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetKitchenOrders()
    {
        var restaurantId = GetRestaurantId();
        var orders = await _orderService.GetKitchenOrdersAsync(restaurantId);
        return Ok(orders);
    }
}
