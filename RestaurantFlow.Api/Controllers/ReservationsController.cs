using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantFlow.Application.DTOs.Reservation;
using RestaurantFlow.Application.Interfaces;

namespace RestaurantFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(IReservationService reservationService, ILogger<ReservationsController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    private int GetRestaurantId() => int.Parse(User.FindFirst("RestaurantId")?.Value ?? "0");

    /// <summary>
    /// Get all reservations
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetReservations()
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var reservations = await _reservationService.GetReservationsAsync(restaurantId);
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
    /// Confirm reservation
    /// </summary>
    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> ConfirmReservation(int id)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var result = await _reservationService.ConfirmReservationAsync(id, restaurantId);
            
            if (!result)
                return NotFound(new { message = "Reservación no encontrada" });

            return Ok(new { message = "Reservación confirmada" });
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
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelReservation(int id)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var result = await _reservationService.CancelReservationAsync(id, restaurantId);
            
            if (!result)
                return NotFound(new { message = "Reservación no encontrada" });

            return Ok(new { message = "Reservación cancelada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar reservación");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Seat reservation
    /// </summary>
    [HttpPost("{id}/seat")]
    public async Task<IActionResult> SeatReservation(int id)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            var result = await _reservationService.SeatReservationAsync(id, restaurantId);
            
            if (!result)
                return NotFound(new { message = "Reservación no encontrada" });

            return Ok(new { message = "Cliente sentado" });
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
    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability([FromQuery] DateTime date, [FromQuery] string startTime, [FromQuery] string endTime, [FromQuery] int numberOfPeople)
    {
        try
        {
            var restaurantId = GetRestaurantId();
            
            var request = new AvailabilityRequestDto
            {
                Date = date,
                StartTime = TimeSpan.Parse(startTime),
                EndTime = TimeSpan.Parse(endTime),
                NumberOfPeople = numberOfPeople
            };

            var availableTables = await _reservationService.GetAvailableTablesAsync(request, restaurantId);
            return Ok(availableTables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar disponibilidad");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
