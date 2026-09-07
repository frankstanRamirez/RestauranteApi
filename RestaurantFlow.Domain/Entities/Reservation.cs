using RestaurantFlow.Domain.Enums;

namespace RestaurantFlow.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public int TableId { get; set; }
    public int? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public int NumberOfPeople { get; set; }
    public DateTime ReservationDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.PENDING;
    public ReservationSource Source { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Restaurant Restaurant { get; set; } = null!;
    public Table Table { get; set; } = null!;
}
