using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Domain.Entities;

namespace RestaurantFlow.Application.Interfaces;

public interface IAppDbContext
{
    // DbSets utilizados por AuthService
    DbSet<User> Users { get; }
    DbSet<Restaurant> Restaurants { get; }
    
    // DbSets utilizados por OrderService  
    DbSet<Order> Orders { get; }
    DbSet<Table> Tables { get; }
    DbSet<Product> Products { get; }
    DbSet<OrderItem> OrderItems { get; }
    
    // DbSets utilizados por ReservationService
    DbSet<Reservation> Reservations { get; }
    
    // Método para guardar cambios
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}