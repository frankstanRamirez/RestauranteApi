using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Application.DTOs.Category;
using RestaurantFlow.Domain.Entities;
using RestaurantFlow.Infrastructure.Data;

namespace RestaurantFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(AppDbContext context, ILogger<CategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private int GetRestaurantId() => int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var categories = await _context.Categories
                .Where(c => c.RestaurantId == restaurantId && c.IsActive)
                .OrderBy(c => c.Name)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var category = await _context.Categories
                .Where(c => c.Id == id && c.RestaurantId == restaurantId)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive
                })
                .FirstOrDefaultAsync();

            if (category == null)
                return NotFound(new { message = "Categoría no encontrada" });

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Create new category
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "El nombre es requerido" });

            var restaurantId = GetRestaurantId();

            // Check if category name already exists in this restaurant
            var existingCategory = await _context.Categories
                .AnyAsync(c => c.RestaurantId == restaurantId && c.Name.ToLower() == dto.Name.ToLower());

            if (existingCategory)
                return BadRequest(new { message = "Ya existe una categoría con este nombre" });

            var category = new Category
            {
                RestaurantId = restaurantId,
                Name = dto.Name,
                Description = dto.Description,
                IsActive = true
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Update category
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "El nombre es requerido" });

            var restaurantId = GetRestaurantId();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurantId);

            if (category == null)
                return NotFound(new { message = "Categoría no encontrada" });

            // Check if name is already used by another category
            var duplicateName = await _context.Categories
                .AnyAsync(c => c.RestaurantId == restaurantId && c.Id != id && c.Name.ToLower() == dto.Name.ToLower());

            if (duplicateName)
                return BadRequest(new { message = "Ya existe una categoría con este nombre" });

            category.Name = dto.Name;
            category.Description = dto.Description;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return Ok(new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Delete category (soft delete - sets IsActive to false)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            var restaurantId = GetRestaurantId();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.RestaurantId == restaurantId);

            if (category == null)
                return NotFound(new { message = "Categoría no encontrada" });

            // Check if category has products
            var productsCount = await _context.Products
                .CountAsync(p => p.CategoryId == id);

            if (productsCount > 0)
                return BadRequest(new { message = "No se puede eliminar una categoría que tiene productos asociados" });

            category.IsActive = false;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Categoría eliminada correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
