using RestaurantFlow.Application.DTOs.Auth;

namespace RestaurantFlow.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    string GenerateJwtToken(int userId, int restaurantId, string role, string name, string email);
}
