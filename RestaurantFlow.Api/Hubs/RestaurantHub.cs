using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace RestaurantFlow.Api.Hubs;

[Authorize]
public class RestaurantHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var restaurantId = Context.User?.FindFirst("RestaurantId")?.Value;
        
        if (!string.IsNullOrEmpty(restaurantId))
        {
            // Unir al usuario al grupo de su restaurante
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Restaurant_{restaurantId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var restaurantId = Context.User?.FindFirst("RestaurantId")?.Value;
        
        if (!string.IsNullOrEmpty(restaurantId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Restaurant_{restaurantId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Método para unirse manualmente a un grupo (opcional)
    public async Task JoinRestaurantGroup(int restaurantId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Restaurant_{restaurantId}");
    }
}
