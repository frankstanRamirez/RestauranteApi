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
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(AppDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private int GetRestaurantId() => int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.RestaurantId == restaurantId)
                .OrderBy(p => p.Category.Name)
                .ThenBy(p => p.Name)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        try
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
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Create new product
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "El nombre es requerido" });

            if (dto.Price <= 0)
                return BadRequest(new { message = "El precio debe ser mayor a 0" });

            var restaurantId = GetRestaurantId();

            // Verify category exists and belongs to the restaurant
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId && c.RestaurantId == restaurantId);

            if (!categoryExists)
                return BadRequest(new { message = "Categoría no encontrada o no pertenece a este restaurante" });

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

            var response = new ProductDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                CategoryName = (await _context.Categories.FindAsync(product.CategoryId))?.Name ?? "",
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                IsAvailable = product.IsAvailable
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Update product
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] CreateProductDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "El nombre es requerido" });

            if (dto.Price <= 0)
                return BadRequest(new { message = "El precio debe ser mayor a 0" });

            var restaurantId = GetRestaurantId();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.RestaurantId == restaurantId);

            if (product == null)
                return NotFound(new { message = "Producto no encontrado" });

            // Verify category exists and belongs to the restaurant
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId && c.RestaurantId == restaurantId);

            if (!categoryExists)
                return BadRequest(new { message = "Categoría no encontrada o no pertenece a este restaurante" });

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.CategoryId = dto.CategoryId;
            product.ImageUrl = dto.ImageUrl;
            product.IsAvailable = dto.IsAvailable;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            var response = new ProductDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                CategoryName = (await _context.Categories.FindAsync(product.CategoryId))?.Name ?? "",
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                IsAvailable = product.IsAvailable
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Delete product
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            var restaurantId = GetRestaurantId();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.RestaurantId == restaurantId);

            if (product == null)
                return NotFound(new { message = "Producto no encontrado" });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Producto eliminado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
