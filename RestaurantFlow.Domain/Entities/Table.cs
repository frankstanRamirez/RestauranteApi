using RestaurantFlow.Domain.Enums;

namespace RestaurantFlow.Domain.Entities;

public class Table
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public int Number { get; set; }
    public int Capacity { get; set; }
    public TableStatus Status { get; set; } = TableStatus.AVAILABLE;
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public TableShape Shape { get; set; } = TableShape.SQUARE;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Restaurant Restaurant { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
