using Microsoft.AspNetCore.Mvc;

namespace RestaurantFlow.Api.Controllers;

[ApiController]
public class HomeController : ControllerBase
{
    /// <summary>
    /// Redirect to Swagger UI documentation
    /// </summary>
    [HttpGet("/")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Index()
    {
        return Redirect("/swagger");
    }

    /// <summary>
    /// Get API information
    /// </summary>
    [HttpGet("/api")]
    public IActionResult ApiInfo()
    {
        return Ok(new
        {
            name = "RestaurantFlow API",
            version = "v1.0",
            description = "REST API for comprehensive restaurant management",
            documentation = "/swagger",
            status = "Online",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
            endpoints = new
            {
                swagger = "/swagger",
                auth = "/api/Auth",
                orders = "/api/Orders",
                reservations = "/api/Reservations",
                products = "/api/Products",
                signalr_hub = "/hubs/restaurant"
            }
        });
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("/health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            service = "RestaurantFlow API",
            version = "1.0.0"
        });
    }
}