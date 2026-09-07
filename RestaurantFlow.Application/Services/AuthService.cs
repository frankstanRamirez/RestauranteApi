using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestaurantFlow.Application.DTOs.Auth;
using RestaurantFlow.Application.Interfaces;
using RestaurantFlow.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RestaurantFlow.Application.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.Restaurant)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !user.IsActive)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var token = GenerateJwtToken(user.Id, user.RestaurantId, user.Role.ToString(), user.Name, user.Email);

        return new LoginResponseDto
        {
            Token = token,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            RestaurantId = user.RestaurantId,
            RestaurantName = user.Restaurant.Name
        };
    }

    public string GenerateJwtToken(int userId, int restaurantId, string role, string name, string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyHereMustBeLongEnough123456"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role),
            new Claim("RestaurantId", restaurantId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "RestaurantFlowApi",
            audience: _configuration["Jwt:Audience"] ?? "RestaurantFlowClient",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
