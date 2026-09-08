using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Application.DTOs.Table;
using RestaurantFlow.Domain.Entities;
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
        var restaurantId = GetRestaurantId();
        var tables = await _context.Tables
            .Where(t => t.RestaurantId == restaurantId)
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

    /// <summary>
    /// Get table by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTable(int id)
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

    /// <summary>
    /// Create new table
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateTable([FromBody] CreateTableDto dto)
    {
        var restaurantId = GetRestaurantId();
        
        var table = new Table
        {
            RestaurantId = restaurantId,
            Number = dto.Number,
            Capacity = dto.Capacity,
            PositionX = dto.PositionX,
            PositionY = dto.PositionY,
            Width = dto.Width,
            Height = dto.Height
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
}
