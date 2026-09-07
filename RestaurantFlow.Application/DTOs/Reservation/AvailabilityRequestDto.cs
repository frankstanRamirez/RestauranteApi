namespace RestaurantFlow.Application.DTOs.Reservation;

public class AvailabilityRequestDto
{
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int NumberOfPeople { get; set; }
}
