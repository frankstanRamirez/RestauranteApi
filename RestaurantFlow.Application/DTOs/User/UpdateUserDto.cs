using RestaurantFlow.Domain.Enums;

namespace RestaurantFlow.Application.DTOs.User;

public class UpdateUserDto
{
    public string Name { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
}
