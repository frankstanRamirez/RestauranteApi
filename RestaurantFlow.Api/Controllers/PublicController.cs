using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Application.DTOs.Category;
using RestaurantFlow.Application.DTOs.Product;
using RestaurantFlow.Application.DTOs.Reservation;
using RestaurantFlow.Application.DTOs.Restaurant;
using RestaurantFlow.Application.DTOs.Table;
using RestaurantFlow.Application.Interfaces;
using RestaurantFlow.Infrastructure.Data;

namespace RestaurantFlow.Api.Controllers;

[ApiController]
[Route("api/public/restaurants")]
public class PublicController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IReservationService _reservationService;

    public PublicController(AppDbContext context, IReservationService reservationService)
    {
        _context = context;
        _reservationService = reservationService;
    }

    /// <summary>
    /// Obtener información del restaurante por slug
    /// </summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetRestaurant(string slug)
    {
        var restaurant = await _context.Restaurants
            .Where(r => r.Slug == slug && r.IsActive)
            .Select(r => new RestaurantDto
            {
                Id = r.Id,
                Name = r.Name,
                Slug = r.Slug,
                Description = r.Description,
                Address = r.Address,
                Phone = r.Phone,
                Email = r.Email,
                LogoUrl = r.LogoUrl,
                CoverImageUrl = r.CoverImageUrl,
                OpeningTime = r.OpeningTime,
                ClosingTime = r.ClosingTime,
                IsActive = r.IsActive
            })
            .FirstOrDefaultAsync();

        if (restaurant == null)
            return NotFound(new { message = "Restaurante no encontrado" });

        return Ok(restaurant);
    }

    /// <summary>
    /// Obtener menú completo
    /// </summary>
    [HttpGet("{slug}/menu")]
    public async Task<IActionResult> GetMenu(string slug)
    {
        var restaurant = await _context.Restaurants
            .Where(r => r.Slug == slug && r.IsActive)
            .FirstOrDefaultAsync();

        if (restaurant == null)
            return NotFound(new { message = "Restaurante no encontrado" });

        var categories = await _context.Categories
            .Where(c => c.RestaurantId == restaurant.Id && c.IsActive)
            .Include(c => c.Products.Where(p => p.IsAvailable))
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                Products = c.Products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    CategoryName = c.Name,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    IsAvailable = p.IsAvailable
                })
            })
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Obtener categorías
    /// </summary>
    [HttpGet("{slug}/categories")]
    public async Task<IActionResult> GetCategories(string slug)
    {
        var restaurant = await _context.Restaurants
            .Where(r => r.Slug == slug && r.IsActive)
            .FirstOrDefaultAsync();

        if (restaurant == null)
            return NotFound(new { message = "Restaurante no encontrado" });

        var categories = await _context.Categories
            .Where(c => c.RestaurantId == restaurant.Id && c.IsActive)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Obtener productos
    /// </summary>
    [HttpGet("{slug}/products")]
    public async Task<IActionResult> GetProducts(string slug)
    {
        var restaurant = await _context.Restaurants
            .Where(r => r.Slug == slug && r.IsActive)
            .FirstOrDefaultAsync();

        if (restaurant == null)
            return NotFound(new { message = "Restaurante no encontrado" });

        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.RestaurantId == restaurant.Id && p.IsAvailable)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                IsAvailable = p.IsAvailable
            })
            .ToListAsync();

        return Ok(products);
    }

    /// <summary>
    /// Obtener mesas
    /// </summary>
    [HttpGet("{slug}/tables")]
    public async Task<IActionResult> GetTables(string slug)
    {
        var restaurant = await _context.Restaurants
            .Where(r => r.Slug == slug && r.IsActive)
            .FirstOrDefaultAsync();

        if (restaurant == null)
            return NotFound(new { message = "Restaurante no encontrado" });

        var tables = await _context.Tables
            .Where(t => t.RestaurantId == restaurant.Id && t.IsActive)
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
    /// Consultar disponibilidad
    /// </summary>
    [HttpGet("{slug}/availability")]
    public async Task<IActionResult> GetAvailability(string slug, [FromQuery] DateTime date, [FromQuery] string startTime, [FromQuery] string endTime, [FromQuery] int numberOfPeople)
    {
        var restaurant = await _context.Restaurants
            .Where(r => r.Slug == slug && r.IsActive)
            .FirstOrDefaultAsync();

        if (restaurant == null)
            return NotFound(new { message = "Restaurante no encontrado" });

        var request = new AvailabilityRequestDto
        {
            Date = date,
            StartTime = TimeSpan.Parse(startTime),
            EndTime = TimeSpan.Parse(endTime),
            NumberOfPeople = numberOfPeople
        };

        var availableTables = await _reservationService.GetAvailableTablesAsync(request, restaurant.Id);
        return Ok(availableTables);
    }

    /// <summary>
    /// Crear reservación desde web
    /// </summary>
    [HttpPost("{slug}/reservations")]
    public async Task<IActionResult> CreateReservation(string slug, [FromBody] CreateReservationDto dto)
    {
        try
        {
            var restaurant = await _context.Restaurants
                .Where(r => r.Slug == slug && r.IsActive)
                .FirstOrDefaultAsync();

            if (restaurant == null)
                return NotFound(new { message = "Restaurante no encontrado" });

            // Forzar fuente WEB
            dto.Source = "WEB";

            var reservation = await _reservationService.CreateReservationAsync(dto, restaurant.Id);
            return CreatedAtAction(nameof(CreateReservation), new { slug, id = reservation.Id }, reservation);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
