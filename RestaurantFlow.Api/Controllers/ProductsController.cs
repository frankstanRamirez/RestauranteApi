using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Application.DTOs.Product;
using RestaurantFlow.Domain.Entities;
using RestaurantFlow.Infrastructure.Data;

namespace RestaurantFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    private int GetRestaurantId() => int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");

    /// <summary>
    /// Obtener todos los productos
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var restaurantId = GetRestaurantId();
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.RestaurantId == restaurantId)
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
    /// Obtener producto por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var restaurantId = GetRestaurantId();
        var product = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Id == id && p.RestaurantId == restaurantId)
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
            .FirstOrDefaultAsync();

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    /// <summary>
    /// Crear nuevo producto
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var restaurantId = GetRestaurantId();
        
        var product = new Product
        {
            RestaurantId = restaurantId,
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            IsAvailable = dto.IsAvailable
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, dto);
    }
}
