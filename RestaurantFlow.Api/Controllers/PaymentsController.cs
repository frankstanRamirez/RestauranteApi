using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Application.DTOs.Payment;
using RestaurantFlow.Domain.Entities;
using RestaurantFlow.Domain.Enums;
using RestaurantFlow.Infrastructure.Data;

namespace RestaurantFlow.Api.Controllers;

[Authorize(Roles = "CASHIER,ADMIN")]
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PaymentsController(AppDbContext context)
    {
        _context = context;
    }

    private int GetRestaurantId() => int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");
    private int GetUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

    /// <summary>
    /// Create payment
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
    {
        var restaurantId = GetRestaurantId();
        var cashierId = GetUserId();

        // Validar orden
        var order = await _context.Orders
            .Include(o => o.Table)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.RestaurantId == restaurantId);

        if (order == null)
            return NotFound(new { message = "Orden no encontrada" });

        if (order.Status == OrderStatus.PAID)
            return BadRequest(new { message = "La orden ya está pagada" });

        // Validar método de pago
        if (!Enum.TryParse<PaymentMethod>(dto.PaymentMethod, out var paymentMethod))
            return BadRequest(new { message = "Método de pago inválido" });

        // Crear pago
        var payment = new Payment
        {
            OrderId = dto.OrderId,
            Amount = dto.Amount,
            PaymentMethod = paymentMethod,
            CashierId = cashierId,
            Reference = dto.Reference
        };

        _context.Payments.Add(payment);

        // Actualizar orden
        order.Status = OrderStatus.PAID;
        order.UpdatedAt = DateTime.UtcNow;

        // Liberar mesa
        order.Table.Status = TableStatus.AVAILABLE;

        await _context.SaveChangesAsync();

        return Ok(new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod.ToString(),
            PaidAt = payment.PaidAt,
            CashierName = User.Identity?.Name ?? "",
            Reference = payment.Reference
        });
    }

    /// <summary>
    /// Get payment by order
    /// </summary>
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetPaymentByOrder(int orderId)
    {
        var restaurantId = GetRestaurantId();
        
        var payment = await _context.Payments
            .Include(p => p.Cashier)
            .Include(p => p.Order)
            .Where(p => p.OrderId == orderId && p.Order.RestaurantId == restaurantId)
            .FirstOrDefaultAsync();

        if (payment == null)
            return NotFound();

        return Ok(new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod.ToString(),
            PaidAt = payment.PaidAt,
            CashierName = payment.Cashier.Name,
            Reference = payment.Reference
        });
    }
}
