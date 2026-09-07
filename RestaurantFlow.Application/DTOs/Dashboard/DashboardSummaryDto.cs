namespace RestaurantFlow.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public decimal SalesToday { get; set; }
    public decimal SalesThisMonth { get; set; }
    public int OrdersToday { get; set; }
    public int PendingOrders { get; set; }
    public int OrdersInKitchen { get; set; }
    public int TablesOccupied { get; set; }
    public int TablesAvailable { get; set; }
    public int ReservationsToday { get; set; }
}
