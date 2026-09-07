using RestaurantFlow.Application.DTOs.Reservation;
using RestaurantFlow.Application.DTOs.Table;

namespace RestaurantFlow.Application.Interfaces;

public interface IReservationService
{
    Task<List<ReservationDto>> GetReservationsAsync(int restaurantId);
    Task<ReservationDto?> GetReservationByIdAsync(int id, int restaurantId);
    Task<ReservationDto> CreateReservationAsync(CreateReservationDto dto, int restaurantId);
    Task<bool> ConfirmReservationAsync(int id, int restaurantId);
    Task<bool> CancelReservationAsync(int id, int restaurantId);
    Task<bool> SeatReservationAsync(int id, int restaurantId);
    Task<List<TableDto>> GetAvailableTablesAsync(AvailabilityRequestDto request, int restaurantId);
}
