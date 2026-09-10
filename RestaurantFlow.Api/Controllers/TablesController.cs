using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Application.DTOs.Table;
using RestaurantFlow.Domain.Entities;
using RestaurantFlow.Domain.Enums;
using RestaurantFlow.Infrastructure.Data;
using System.Security.Claims;

namespace RestaurantFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TablesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<TablesController> _logger;

    public TablesController(AppDbContext context, ILogger<TablesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private int GetRestaurantId() => int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");

    /// <summary>
    /// Get all tables
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTables()
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var tables = await _context.Tables
                .Where(t => t.RestaurantId == restaurantId && t.IsActive)
                .OrderBy(t => t.Number)
                .Select(t => new TableDto
                {
                    Id = t.Id,
                    Number = t.Number,
                    Capacity = t.Capacity,
                    Status = t.Status.ToString(),
                    PositionX = t.PositionX,
                    PositionY = t.PositionY,
                    Width = t.Width,
                    Height = t.Height,
                    Shape = t.Shape.ToString(),
                    IsActive = t.IsActive
                })
                .ToListAsync();

            return Ok(tables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tables");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Get table by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTable(int id)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var table = await _context.Tables
                .Where(t => t.Id == id && t.RestaurantId == restaurantId)
                .Select(t => new TableDto
                {
                    Id = t.Id,
                    Number = t.Number,
                    Capacity = t.Capacity,
                    Status = t.Status.ToString(),
                    PositionX = t.PositionX,
                    PositionY = t.PositionY,
                    Width = t.Width,
                    Height = t.Height,
                    Shape = t.Shape.ToString(),
                    IsActive = t.IsActive
                })
                .FirstOrDefaultAsync();

            if (table == null)
                return NotFound(new { message = "Mesa no encontrada" });

            return Ok(table);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Create new table
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateTable([FromBody] CreateTableDto dto)
    {
        try
        {
            if (dto.Number <= 0)
                return BadRequest(new { message = "El número de mesa debe ser mayor a 0" });

            if (dto.Capacity <= 0)
                return BadRequest(new { message = "La capacidad debe ser mayor a 0" });

            var restaurantId = GetRestaurantId();

            // Check if table number already exists
            var existingTable = await _context.Tables
                .AnyAsync(t => t.RestaurantId == restaurantId && t.Number == dto.Number);

            if (existingTable)
                return BadRequest(new { message = "Ya existe una mesa con este número" });

            var table = new Table
            {
                RestaurantId = restaurantId,
                Number = dto.Number,
                Capacity = dto.Capacity,
                PositionX = dto.PositionX,
                PositionY = dto.PositionY,
                Width = dto.Width,
                Height = dto.Height,
                Shape = Enum.TryParse<TableShape>(dto.Shape.ToString(), out var shape) ? shape : TableShape.SQUARE,
                Status = TableStatus.AVAILABLE,
                IsActive = true
            };

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTable), new { id = table.Id }, new TableDto
            {
                Id = table.Id,
                Number = table.Number,
                Capacity = table.Capacity,
                Status = table.Status.ToString(),
                PositionX = table.PositionX,
                PositionY = table.PositionY,
                Width = table.Width,
                Height = table.Height,
                Shape = table.Shape.ToString(),
                IsActive = table.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating table");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Update table
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateTable(int id, [FromBody] CreateTableDto dto)
    {
        try
        {
            if (dto.Number <= 0)
                return BadRequest(new { message = "El número de mesa debe ser mayor a 0" });

            if (dto.Capacity <= 0)
                return BadRequest(new { message = "La capacidad debe ser mayor a 0" });

            var restaurantId = GetRestaurantId();

            var table = await _context.Tables
                .FirstOrDefaultAsync(t => t.Id == id && t.RestaurantId == restaurantId);

            if (table == null)
                return NotFound(new { message = "Mesa no encontrada" });

            // Check if new table number is already used by another table
            var duplicateNumber = await _context.Tables
                .AnyAsync(t => t.RestaurantId == restaurantId && t.Id != id && t.Number == dto.Number);

            if (duplicateNumber)
                return BadRequest(new { message = "Ya existe otra mesa con este número" });

            table.Number = dto.Number;
            table.Capacity = dto.Capacity;
            table.PositionX = dto.PositionX;
            table.PositionY = dto.PositionY;
            table.Width = dto.Width;
            table.Height = dto.Height;
            table.Shape = Enum.TryParse<TableShape>(dto.Shape.ToString(), out var shape) ? shape : TableShape.SQUARE;

            _context.Tables.Update(table);
            await _context.SaveChangesAsync();

            return Ok(new TableDto
            {
                Id = table.Id,
                Number = table.Number,
                Capacity = table.Capacity,
                Status = table.Status.ToString(),
                PositionX = table.PositionX,
                PositionY = table.PositionY,
                Width = table.Width,
                Height = table.Height,
                Shape = table.Shape.ToString(),
                IsActive = table.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating table");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Change table status (AVAILABLE, OCCUPIED, RESERVED, MAINTENANCE)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "ADMIN,WAITER")]
    public async Task<IActionResult> UpdateTableStatus(int id, [FromBody] UpdateTableStatusDto dto)
    {
        try
        {
            var restaurantId = GetRestaurantId();

            var table = await _context.Tables
                .FirstOrDefaultAsync(t => t.Id == id && t.RestaurantId == restaurantId);

            if (table == null)
                return NotFound(new { message = "Mesa no encontrada" });

            // Validate status string
            if (!Enum.TryParse<TableStatus>(dto.Status, out var newStatus))
                return BadRequest(new { message = "Estado de mesa inválido" });

            table.Status = newStatus;

            _context.Tables.Update(table);
            await _context.SaveChangesAsync();

            return Ok(new TableDto
            {
                Id = table.Id,
                Number = table.Number,
                Capacity = table.Capacity,
                Status = table.Status.ToString(),
                PositionX = table.PositionX,
                PositionY = table.PositionY,
                Width = table.Width,
                Height = table.Height,
                Shape = table.Shape.ToString(),
                IsActive = table.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating table status");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
