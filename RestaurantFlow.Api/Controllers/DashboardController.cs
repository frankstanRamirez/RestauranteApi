using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Application.DTOs.Dashboard;
using RestaurantFlow.Domain.Enums;
using RestaurantFlow.Infrastructure.Data;

namespace RestaurantFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    private int GetRestaurantId() => int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");

    /// <summary>
    /// Obtener resumen del dashboard
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var restaurantId = GetRestaurantId();
        var today = DateTime.Today;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        // Ventas del día
        var salesToday = await _context.Orders
            .Where(o => o.RestaurantId == restaurantId && 
                       o.Status == OrderStatus.PAID && 
                       o.CreatedAt.Date == today)
            .SumAsync(o => (decimal?)o.Total) ?? 0;

        // Ventas del mes
        var salesThisMonth = await _context.Orders
            .Where(o => o.RestaurantId == restaurantId && 
                       o.Status == OrderStatus.PAID && 
                       o.CreatedAt >= startOfMonth)
            .SumAsync(o => (decimal?)o.Total) ?? 0;

        // Órdenes del día
        var ordersToday = await _context.Orders
            .CountAsync(o => o.RestaurantId == restaurantId && 
                            o.CreatedAt.Date == today);

        // Órdenes pendientes
        var pendingOrders = await _context.Orders
            .CountAsync(o => o.RestaurantId == restaurantId && 
                            (o.Status == OrderStatus.PENDING || o.Status == OrderStatus.WAITING_PAYMENT));

        // Órdenes en cocina
        var ordersInKitchen = await _context.Orders
            .CountAsync(o => o.RestaurantId == restaurantId && 
                            (o.Status == OrderStatus.SENT_TO_KITCHEN || 
                             o.Status == OrderStatus.PREPARING || 
                             o.Status == OrderStatus.READY));

        // Mesas ocupadas
        var tablesOccupied = await _context.Tables
            .CountAsync(t => t.RestaurantId == restaurantId && 
                            t.Status == TableStatus.OCCUPIED);

        // Mesas disponibles
        var tablesAvailable = await _context.Tables
            .CountAsync(t => t.RestaurantId == restaurantId && 
                            t.Status == TableStatus.AVAILABLE);

        // Reservas del día
        var reservationsToday = await _context.Reservations
            .CountAsync(r => r.RestaurantId == restaurantId && 
                            r.ReservationDate.Date == today &&
                            r.Status != ReservationStatus.CANCELLED);

        var summary = new DashboardSummaryDto
        {
            SalesToday = salesToday,
            SalesThisMonth = salesThisMonth,
            OrdersToday = ordersToday,
            PendingOrders = pendingOrders,
            OrdersInKitchen = ordersInKitchen,
            TablesOccupied = tablesOccupied,
            TablesAvailable = tablesAvailable,
            ReservationsToday = reservationsToday
        };

        return Ok(summary);
    }

    /// <summary>
    /// Obtener productos más vendidos
    /// </summary>
    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int limit = 10)
    {
        var restaurantId = GetRestaurantId();

        var topProducts = await _context.OrderItems
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.RestaurantId == restaurantId && 
                        oi.Order.Status == OrderStatus.PAID)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                QuantitySold = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.Subtotal)
            })
            .OrderByDescending(p => p.QuantitySold)
            .Take(limit)
            .ToListAsync();

        return Ok(topProducts);
    }

    /// <summary>
    /// Obtener ventas por fecha
    /// </summary>
    [HttpGet("sales")]
    public async Task<IActionResult> GetSales([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var restaurantId = GetRestaurantId();
        var start = startDate ?? DateTime.Today.AddDays(-30);
        var end = endDate ?? DateTime.Today;

        var sales = await _context.Orders
            .Where(o => o.RestaurantId == restaurantId && 
                       o.Status == OrderStatus.PAID && 
                       o.CreatedAt.Date >= start.Date && 
                       o.CreatedAt.Date <= end.Date)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                TotalSales = g.Sum(o => o.Total),
                OrderCount = g.Count()
            })
            .OrderBy(s => s.Date)
            .ToListAsync();

        return Ok(sales);
    }
}
