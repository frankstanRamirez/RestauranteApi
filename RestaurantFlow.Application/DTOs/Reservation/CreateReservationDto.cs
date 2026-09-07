namespace RestaurantFlow.Application.DTOs.Reservation;

public class CreateReservationDto
{
    public int TableId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public int NumberOfPeople { get; set; }
    public DateTime ReservationDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Source { get; set; } = "WEB";
    public string? Notes { get; set; }
}
