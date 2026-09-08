using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Application.DTOs.Reservation;
using RestaurantFlow.Application.DTOs.Table;
using RestaurantFlow.Application.Interfaces;
using RestaurantFlow.Domain.Entities;
using RestaurantFlow.Domain.Enums;

namespace RestaurantFlow.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IAppDbContext _context;

    public ReservationService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReservationDto>> GetReservationsAsync(int restaurantId)
    {
        var reservations = await _context.Reservations
            .Include(r => r.Table)
            .Where(r => r.RestaurantId == restaurantId)
            .OrderByDescending(r => r.ReservationDate)
            .ThenBy(r => r.StartTime)
            .ToListAsync();

        return reservations.Select(MapToDto).ToList();
    }

    public async Task<ReservationDto?> GetReservationByIdAsync(int id, int restaurantId)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Table)
            .FirstOrDefaultAsync(r => r.Id == id && r.RestaurantId == restaurantId);

        return reservation == null ? null : MapToDto(reservation);
    }

    public async Task<ReservationDto> CreateReservationAsync(CreateReservationDto dto, int restaurantId)
    {
        // Validar mesa
        var table = await _context.Tables
            .FirstOrDefaultAsync(t => t.Id == dto.TableId && t.RestaurantId == restaurantId && t.IsActive);

        if (table == null)
            throw new Exception("Mesa no encontrada o inactiva");

        // Validar capacidad
        if (dto.NumberOfPeople > table.Capacity)
            throw new Exception($"La mesa solo tiene capacidad para {table.Capacity} personas");

        // Validar fecha (no puede ser en el pasado)
        if (dto.ReservationDate.Date < DateTime.Today)
            throw new Exception("No se pueden hacer reservas en fechas pasadas");

        // Validar horario del restaurante
        var restaurant = await _context.Restaurants.FindAsync(restaurantId);
        if (restaurant != null && restaurant.OpeningTime.HasValue && restaurant.ClosingTime.HasValue)
        {
            if (dto.StartTime < restaurant.OpeningTime.Value || dto.EndTime > restaurant.ClosingTime.Value)
                throw new Exception("La reserva está fuera del horario del restaurante");
        }

        // Validar que no haya solapamiento
        var hasOverlap = await _context.Reservations
            .AnyAsync(r => r.RestaurantId == restaurantId &&
                          r.TableId == dto.TableId &&
                          r.ReservationDate.Date == dto.ReservationDate.Date &&
                          r.Status != ReservationStatus.CANCELLED &&
                          r.Status != ReservationStatus.COMPLETED &&
                          ((dto.StartTime >= r.StartTime && dto.StartTime < r.EndTime) ||
                           (dto.EndTime > r.StartTime && dto.EndTime <= r.EndTime) ||
                           (dto.StartTime <= r.StartTime && dto.EndTime >= r.EndTime)));

        if (hasOverlap)
            throw new Exception("Ya existe una reserva en ese horario para esta mesa");

        // Parsear fuente
        if (!Enum.TryParse<ReservationSource>(dto.Source, out var source))
            source = ReservationSource.WEB;

        // Crear reservación
        var reservation = new Reservation
        {
            RestaurantId = restaurantId,
            TableId = dto.TableId,
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            CustomerEmail = dto.CustomerEmail,
            NumberOfPeople = dto.NumberOfPeople,
            ReservationDate = dto.ReservationDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = ReservationStatus.PENDING,
            Source = source,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return await GetReservationByIdAsync(reservation.Id, restaurantId) ?? throw new Exception("Error al crear reservación");
    }

    public async Task<bool> ConfirmReservationAsync(int id, int restaurantId)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id && r.RestaurantId == restaurantId);

        if (reservation == null)
            return false;

        reservation.Status = ReservationStatus.CONFIRMED;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelReservationAsync(int id, int restaurantId)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id && r.RestaurantId == restaurantId);

        if (reservation == null)
            return false;

        reservation.Status = ReservationStatus.CANCELLED;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SeatReservationAsync(int id, int restaurantId)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Table)
            .FirstOrDefaultAsync(r => r.Id == id && r.RestaurantId == restaurantId);

        if (reservation == null)
            return false;

        reservation.Status = ReservationStatus.SEATED;
        reservation.Table.Status = TableStatus.OCCUPIED;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<TableDto>> GetAvailableTablesAsync(AvailabilityRequestDto request, int restaurantId)
    {
        // Obtener todas las mesas activas del restaurante
        var allTables = await _context.Tables
            .Where(t => t.RestaurantId == restaurantId && t.IsActive && t.Capacity >= request.NumberOfPeople)
            .ToListAsync();

        // Obtener reservas que se solapan
        var overlappingReservations = await _context.Reservations
            .Where(r => r.RestaurantId == restaurantId &&
                       r.ReservationDate.Date == request.Date.Date &&
                       r.Status != ReservationStatus.CANCELLED &&
                       r.Status != ReservationStatus.COMPLETED &&
                       ((request.StartTime >= r.StartTime && request.StartTime < r.EndTime) ||
                        (request.EndTime > r.StartTime && request.EndTime <= r.EndTime) ||
                        (request.StartTime <= r.StartTime && request.EndTime >= r.EndTime)))
            .Select(r => r.TableId)
            .ToListAsync();

        // Filtrar mesas disponibles
        var availableTables = allTables
            .Where(t => !overlappingReservations.Contains(t.Id))
            .Select(t => new TableDto
            {
                Id = t.Id,
                Number = t.Number,
                Capacity = t.Capacity,
                Status = t.Status.ToString(),
                PositionX = t.PositionX,
                PositionY = t.PositionY,
                Width = t.Width,
                Height = t.Height,
                Shape = t.Shape.ToString(),
                IsActive = t.IsActive
            })
            .ToList();

        return availableTables;
    }

    private static ReservationDto MapToDto(Reservation reservation)
    {
        return new ReservationDto
        {
            Id = reservation.Id,
            TableId = reservation.TableId,
            TableNumber = reservation.Table.Number,
            CustomerName = reservation.CustomerName,
            CustomerPhone = reservation.CustomerPhone,
            CustomerEmail = reservation.CustomerEmail,
            NumberOfPeople = reservation.NumberOfPeople,
            ReservationDate = reservation.ReservationDate,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            Status = reservation.Status.ToString(),
            Source = reservation.Source.ToString(),
            Notes = reservation.Notes,
            CreatedAt = reservation.CreatedAt
        };
    }
}
