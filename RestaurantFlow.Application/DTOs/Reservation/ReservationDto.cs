namespace RestaurantFlow.Application.DTOs.Reservation;

public class ReservationDto
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public int TableNumber { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public int NumberOfPeople { get; set; }
    public DateTime ReservationDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
