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

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    private int GetRestaurantId() => int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var restaurantId = GetRestaurantId();
        var categories = await _context.Categories
            .Where(c => c.RestaurantId == restaurantId)
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
    /// Create new category
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var restaurantId = GetRestaurantId();
        
        var category = new Category
        {
            RestaurantId = restaurantId,
            Name = dto.Name,
            Description = dto.Description
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive
        });
    }
}
