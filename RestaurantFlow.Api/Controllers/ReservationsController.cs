using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Application.DTOs.Reservation;
using RestaurantFlow.Application.Interfaces;
using RestaurantFlow.Domain.Enums;
using RestaurantFlow.Infrastructure.Data;

namespace RestaurantFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly AppDbContext _context;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(IReservationService reservationService, AppDbContext context, ILogger<ReservationsController> logger)
    {
        _reservationService = reservationService;
        _context = context;
        _logger = logger;
    }

    private int GetRestaurantId() => int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");

    /// <summary>
    /// Get all reservations (with optional status filter)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetReservations([FromQuery] string? status = null)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var reservations = await _reservationService.GetReservationsAsync(restaurantId);

            if (!string.IsNullOrEmpty(status))
            {
                reservations = reservations.Where(r => r.Status == status).ToList();
            }

            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reservaciones");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Get reservation by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReservation(int id)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var reservation = await _reservationService.GetReservationByIdAsync(id, restaurantId);

            if (reservation == null)
                return NotFound(new { message = "Reservación no encontrada" });

            return Ok(reservation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reservación");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Create new reservation
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.CustomerName))
                return BadRequest(new { message = "El nombre del cliente es requerido" });

            if (string.IsNullOrWhiteSpace(dto.CustomerPhone))
                return BadRequest(new { message = "El teléfono del cliente es requerido" });

            if (dto.NumberOfPeople <= 0)
                return BadRequest(new { message = "El número de personas debe ser mayor a 0" });

            var restaurantId = GetRestaurantId();
            var reservation = await _reservationService.CreateReservationAsync(dto, restaurantId);
            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear reservación");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update reservation
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,WAITER")]
    public async Task<IActionResult> UpdateReservation(int id, [FromBody] CreateReservationDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.CustomerName))
                return BadRequest(new { message = "El nombre del cliente es requerido" });

            if (dto.NumberOfPeople <= 0)
                return BadRequest(new { message = "El número de personas debe ser mayor a 0" });

            var restaurantId = GetRestaurantId();

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id && r.RestaurantId == restaurantId);

            if (reservation == null)
                return NotFound(new { message = "Reservación no encontrada" });

            // Prevent updating cancelled or completed reservations
            if (reservation.Status == ReservationStatus.CANCELLED || reservation.Status == ReservationStatus.COMPLETED)
                return BadRequest(new { message = "No se puede editar una reservación cancelada o completada" });

            // Verify table exists
            var tableExists = await _context.Tables
                .AnyAsync(t => t.Id == dto.TableId && t.RestaurantId == restaurantId);

            if (!tableExists)
                return BadRequest(new { message = "Mesa no encontrada" });

            reservation.TableId = dto.TableId;
            reservation.CustomerName = dto.CustomerName;
            reservation.CustomerPhone = dto.CustomerPhone;
            reservation.CustomerEmail = dto.CustomerEmail;
            reservation.NumberOfPeople = dto.NumberOfPeople;
            reservation.ReservationDate = dto.ReservationDate;
            reservation.StartTime = dto.StartTime;
            reservation.EndTime = dto.EndTime;
            reservation.Notes = dto.Notes;

            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();

            var updated = await _reservationService.GetReservationByIdAsync(id, restaurantId);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar reservación");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Confirm reservation
    /// </summary>
    [HttpPut("{id}/confirm")]
    [Authorize(Roles = "ADMIN,WAITER")]
    public async Task<IActionResult> ConfirmReservation(int id)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var result = await _reservationService.ConfirmReservationAsync(id, restaurantId);

            if (!result)
                return NotFound(new { message = "Reservación no encontrada" });

            var reservation = await _reservationService.GetReservationByIdAsync(id, restaurantId);
            return Ok(new { message = "Reservación confirmada", reservation });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al confirmar reservación");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Cancel reservation
    /// </summary>
    [HttpPut("{id}/cancel")]
    [Authorize(Roles = "ADMIN,WAITER")]
    public async Task<IActionResult> CancelReservation(int id)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var result = await _reservationService.CancelReservationAsync(id, restaurantId);

            if (!result)
                return NotFound(new { message = "Reservación no encontrada" });

            var reservation = await _reservationService.GetReservationByIdAsync(id, restaurantId);
            return Ok(new { message = "Reservación cancelada", reservation });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar reservación");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Seat reservation (check-in)
    /// </summary>
    [HttpPut("{id}/seat")]
    [Authorize(Roles = "ADMIN,WAITER")]
    public async Task<IActionResult> SeatReservation(int id)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var result = await _reservationService.SeatReservationAsync(id, restaurantId);

            if (!result)
                return NotFound(new { message = "Reservación no encontrada" });

            var reservation = await _reservationService.GetReservationByIdAsync(id, restaurantId);
            return Ok(new { message = "Cliente sentado", reservation });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al sentar cliente");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Check availability
    /// </summary>
    [HttpPost("check-availability")]
    public async Task<IActionResult> CheckAvailability([FromBody] AvailabilityRequestDto request)
    {
        try
        {
            if (request.NumberOfPeople <= 0)
                return BadRequest(new { message = "El número de personas debe ser mayor a 0" });

            var restaurantId = GetRestaurantId();
            var availableTables = await _reservationService.GetAvailableTablesAsync(request, restaurantId);

            return Ok(new
            {
                date = request.Date,
                startTime = request.StartTime,
                endTime = request.EndTime,
                numberOfPeople = request.NumberOfPeople,
                availableTables = availableTables
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar disponibilidad");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Get upcoming reservations (within next N days)
    /// </summary>
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingReservations([FromQuery] int days = 7)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(days);

            var upcoming = await _context.Reservations
                .Include(r => r.Table)
                .Where(r => r.RestaurantId == restaurantId &&
                           r.ReservationDate >= startDate &&
                           r.ReservationDate <= endDate &&
                           (r.Status == ReservationStatus.PENDING || r.Status == ReservationStatus.CONFIRMED))
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    TableId = r.TableId,
                    TableNumber = r.Table.Number,
                    CustomerName = r.CustomerName,
                    CustomerPhone = r.CustomerPhone,
                    CustomerEmail = r.CustomerEmail,
                    NumberOfPeople = r.NumberOfPeople,
                    ReservationDate = r.ReservationDate,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    Status = r.Status.ToString(),
                    Source = r.Source.ToString(),
                    Notes = r.Notes,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(upcoming);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reservaciones próximas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}


