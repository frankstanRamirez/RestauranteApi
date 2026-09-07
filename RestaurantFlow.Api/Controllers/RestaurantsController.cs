using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Application.DTOs.Restaurant;
using RestaurantFlow.Infrastructure.Data;

namespace RestaurantFlow.Api.Controllers;

[Authorize(Roles = "ADMIN")]
[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly AppDbContext _context;

    public RestaurantsController(AppDbContext context)
    {
        _context = context;
    }

    private int GetRestaurantId() => int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");

    /// <summary>
    /// Obtener información del restaurante actual
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRestaurant()
    {
        var restaurantId = GetRestaurantId();
        var restaurant = await _context.Restaurants
            .Where(r => r.Id == restaurantId)
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
            return NotFound();

        return Ok(restaurant);
    }

    /// <summary>
    /// Actualizar información del restaurante
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateRestaurant([FromBody] RestaurantDto dto)
    {
        var restaurantId = GetRestaurantId();
        var restaurant = await _context.Restaurants.FindAsync(restaurantId);

        if (restaurant == null)
            return NotFound();

        restaurant.Name = dto.Name;
        restaurant.Description = dto.Description;
        restaurant.Address = dto.Address;
        restaurant.Phone = dto.Phone;
        restaurant.Email = dto.Email;
        restaurant.LogoUrl = dto.LogoUrl;
        restaurant.CoverImageUrl = dto.CoverImageUrl;
        restaurant.OpeningTime = dto.OpeningTime;
        restaurant.ClosingTime = dto.ClosingTime;

        await _context.SaveChangesAsync();

        return Ok(dto);
    }
}
