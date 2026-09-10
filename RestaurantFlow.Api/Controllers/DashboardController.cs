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
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(AppDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private int GetRestaurantId() => int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");

    /// <summary>
    /// Get complete dashboard summary
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        try
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

            // Mesas totales
            var totalTables = await _context.Tables
                .CountAsync(t => t.RestaurantId == restaurantId && t.IsActive);

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
                TotalTables = totalTables,
                ReservationsToday = reservationsToday
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard summary");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Get orders summary by status
    /// </summary>
    [HttpGet("orders-summary")]
    public async Task<IActionResult> GetOrdersSummary()
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var today = DateTime.Today;

            var summary = new
            {
                pending = await _context.Orders.CountAsync(o => o.RestaurantId == restaurantId && o.Status == OrderStatus.PENDING),
                sentToKitchen = await _context.Orders.CountAsync(o => o.RestaurantId == restaurantId && o.Status == OrderStatus.SENT_TO_KITCHEN),
                preparing = await _context.Orders.CountAsync(o => o.RestaurantId == restaurantId && o.Status == OrderStatus.PREPARING),
                ready = await _context.Orders.CountAsync(o => o.RestaurantId == restaurantId && o.Status == OrderStatus.READY),
                delivered = await _context.Orders.CountAsync(o => o.RestaurantId == restaurantId && o.Status == OrderStatus.DELIVERED),
                waitingPayment = await _context.Orders.CountAsync(o => o.RestaurantId == restaurantId && o.Status == OrderStatus.WAITING_PAYMENT),
                paid = await _context.Orders.CountAsync(o => o.RestaurantId == restaurantId && o.Status == OrderStatus.PAID),
                cancelled = await _context.Orders.CountAsync(o => o.RestaurantId == restaurantId && o.Status == OrderStatus.CANCELLED),
                todayTotal = await _context.Orders.CountAsync(o => o.RestaurantId == restaurantId && o.CreatedAt.Date == today)
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders summary");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Get revenue by period
    /// </summary>
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] string period = "month", [FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var currentYear = year ?? DateTime.Today.Year;
            var currentMonth = month ?? DateTime.Today.Month;

            object result = period.ToLower() switch
            {
                "day" => await GetDailyRevenue(restaurantId, currentYear, currentMonth),
                "week" => await GetWeeklyRevenue(restaurantId),
                "month" => await GetMonthlyRevenue(restaurantId, currentYear),
                "year" => await GetYearlyRevenue(restaurantId),
                _ => new { error = "Período inválido. Use: day, week, month, o year" }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    private async Task<object> GetDailyRevenue(int restaurantId, int year, int month)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var data = new List<object>();

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);
            var revenue = await _context.Orders
                .Where(o => o.RestaurantId == restaurantId &&
                           o.Status == OrderStatus.PAID &&
                           o.CreatedAt.Date == date)
                .SumAsync(o => (decimal?)o.Total) ?? 0;

            data.Add(new { date = date.ToShortDateString(), revenue });
        }

        return new { period = $"Day - {year}/{month:D2}", data };
    }

    private async Task<object> GetWeeklyRevenue(int restaurantId)
    {
        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var data = new List<object>();

        for (int i = 0; i < 7; i++)
        {
            var date = startOfWeek.AddDays(i);
            var revenue = await _context.Orders
                .Where(o => o.RestaurantId == restaurantId &&
                           o.Status == OrderStatus.PAID &&
                           o.CreatedAt.Date == date)
                .SumAsync(o => (decimal?)o.Total) ?? 0;

            data.Add(new { date = date.ToShortDateString(), dayOfWeek = date.DayOfWeek.ToString(), revenue });
        }

        return new { period = "Week", data };
    }

    private async Task<object> GetMonthlyRevenue(int restaurantId, int year)
    {
        var data = new List<object>();

        for (int month = 1; month <= 12; month++)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var revenue = await _context.Orders
                .Where(o => o.RestaurantId == restaurantId &&
                           o.Status == OrderStatus.PAID &&
                           o.CreatedAt.Date >= startDate &&
                           o.CreatedAt.Date <= endDate)
                .SumAsync(o => (decimal?)o.Total) ?? 0;

            data.Add(new { month = startDate.ToString("MMM"), revenue });
        }

        return new { period = $"Year {year}", data };
    }

    private async Task<object> GetYearlyRevenue(int restaurantId)
    {
        var data = new List<object>();
        var currentYear = DateTime.Today.Year;

        for (int year = currentYear - 4; year <= currentYear; year++)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = startDate.AddYears(1).AddDays(-1);

            var revenue = await _context.Orders
                .Where(o => o.RestaurantId == restaurantId &&
                           o.Status == OrderStatus.PAID &&
                           o.CreatedAt.Date >= startDate &&
                           o.CreatedAt.Date <= endDate)
                .SumAsync(o => (decimal?)o.Total) ?? 0;

            data.Add(new { year, revenue });
        }

        return new { period = "Year", data };
    }

    /// <summary>
    /// Get top products
    /// </summary>
    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int limit = 10)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top products");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Get sales by date range
    /// </summary>
    [HttpGet("sales")]
    public async Task<IActionResult> GetSales([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
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
                    date = g.Key,
                    totalSales = g.Sum(o => o.Total),
                    orderCount = g.Count()
                })
                .OrderBy(s => s.date)
                .ToListAsync();

            return Ok(sales);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Get table statistics
    /// </summary>
    [HttpGet("tables-stats")]
    public async Task<IActionResult> GetTablesStats()
    {
        try
        {
            var restaurantId = GetRestaurantId();

            var stats = new
            {
                total = await _context.Tables.CountAsync(t => t.RestaurantId == restaurantId && t.IsActive),
                available = await _context.Tables.CountAsync(t => t.RestaurantId == restaurantId && t.Status == TableStatus.AVAILABLE),
                occupied = await _context.Tables.CountAsync(t => t.RestaurantId == restaurantId && t.Status == TableStatus.OCCUPIED),
                reserved = await _context.Tables.CountAsync(t => t.RestaurantId == restaurantId && t.Status == TableStatus.RESERVED),
                maintenance = await _context.Tables.CountAsync(t => t.RestaurantId == restaurantId && t.Status == TableStatus.MAINTENANCE)
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tables stats");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Get average order value
    /// </summary>
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var today = DateTime.Today;

            var paidOrders = await _context.Orders
                .Where(o => o.RestaurantId == restaurantId && o.Status == OrderStatus.PAID)
                .ToListAsync();

            var todayOrders = await _context.Orders
                .Where(o => o.RestaurantId == restaurantId && o.CreatedAt.Date == today)
                .ToListAsync();

            var metrics = new
            {
                averageOrderValue = paidOrders.Any() ? paidOrders.Average(o => o.Total) : 0,
                ordersToday = todayOrders.Count(),
                averageItemsPerOrder = paidOrders.Any() ? (double)paidOrders.Sum(o => o.OrderItems.Count) / paidOrders.Count() : 0,
                totalPaidOrders = paidOrders.Count(),
                totalRevenue = paidOrders.Sum(o => o.Total)
            };

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metrics");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
