using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Application.DTOs.User;
using RestaurantFlow.Domain.Entities;
using RestaurantFlow.Domain.Enums;
using RestaurantFlow.Infrastructure.Data;
using BCrypt.Net;

namespace RestaurantFlow.Api.Controllers;

[Authorize(Roles = "ADMIN")]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(AppDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get the RestaurantId from the authenticated user's token
    /// </summary>
    private int GetRestaurantId() => int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");

    /// <summary>
    /// Get the UserId from the authenticated user's token
    /// </summary>
    private int GetUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

    /// <summary>
    /// List all users for the admin's restaurant
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var restaurantId = GetRestaurantId();

            var users = await _context.Users
                .Where(u => u.RestaurantId == restaurantId)
                .OrderBy(u => u.Name)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing users");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Create a new user in the admin's restaurant
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "El nombre es requerido" });

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "El email es requerido" });

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "La contraseña es requerida" });

            if (dto.Password.Length < 6)
                return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres" });

            var restaurantId = GetRestaurantId();

            // Check if email already exists in this restaurant
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.RestaurantId == restaurantId && u.Email == dto.Email);

            if (existingUser != null)
                return BadRequest(new { message = "El email ya existe en este restaurante" });

            // Create the new user
            var newUser = new User
            {
                RestaurantId = restaurantId,
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                Id = newUser.Id,
                Name = newUser.Name,
                Email = newUser.Email,
                Role = newUser.Role.ToString(),
                IsActive = newUser.IsActive,
                CreatedAt = newUser.CreatedAt
            };

            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Get a specific user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        try
        {
            var restaurantId = GetRestaurantId();

            var user = await _context.Users
                .Where(u => u.Id == id && u.RestaurantId == restaurantId)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Update user name, role, or active status
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "El nombre es requerido" });

            var restaurantId = GetRestaurantId();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.RestaurantId == restaurantId);

            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            // Prevent updating your own role
            var currentUserId = GetUserId();
            if (user.Id == currentUserId && user.Role != dto.Role)
                return BadRequest(new { message = "No puedes cambiar tu propio rol" });

            user.Name = dto.Name;
            user.Role = dto.Role;
            user.IsActive = dto.IsActive;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Deactivate a user (soft delete - preserves history)
    /// </summary>
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var currentUserId = GetUserId();

            // Prevent deactivating yourself
            if (id == currentUserId)
                return BadRequest(new { message = "No puedes desactivar tu propia cuenta" });

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.RestaurantId == restaurantId);

            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            if (!user.IsActive)
                return BadRequest(new { message = "El usuario ya está desactivado" });

            user.IsActive = false;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
