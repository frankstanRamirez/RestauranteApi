using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Domain.Entities;
using RestaurantFlow.Domain.Enums;
using BCrypt.Net;

namespace RestaurantFlow.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Restaurants.AnyAsync())
            return; // Ya hay datos

        // Crear Restaurante
        var restaurant = new Restaurant
        {
            Name = "Villa El Paraíso",
            Slug = "villa-el-paraiso",
            Description = "Restaurante tradicional en Caluco, El Salvador. Especialidad en comida típica salvadoreña.",
            Address = "Caluco, Sonsonate, El Salvador",
            Phone = "+503 2450-1234",
            Email = "info@villaelparaiso.com",
            OpeningTime = new TimeSpan(8, 0, 0),
            ClosingTime = new TimeSpan(21, 0, 0),
            IsActive = true
        };
        context.Restaurants.Add(restaurant);
        await context.SaveChangesAsync();

        // Crear Usuarios con credenciales simples para demo
        var users = new List<User>
        {
            new User
            {
                RestaurantId = restaurant.Id,
                Name = "Administrador",
                Email = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = UserRole.ADMIN,
                IsActive = true
            },
            new User
            {
                RestaurantId = restaurant.Id,
                Name = "Mesero Demo",
                Email = "mesero",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("mesero123"),
                Role = UserRole.WAITER,
                IsActive = true
            },
            new User
            {
                RestaurantId = restaurant.Id,
                Name = "Cocina Demo",
                Email = "cocina",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("cocina123"),
                Role = UserRole.KITCHEN,
                IsActive = true
            }
        };
        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // Crear 10 Mesas con posiciones visuales
        var tables = new List<Table>
        {
            new Table { RestaurantId = restaurant.Id, Number = 1, Capacity = 4, Status = TableStatus.AVAILABLE, PositionX = 100, PositionY = 100, Width = 80, Height = 80, Shape = TableShape.SQUARE },
            new Table { RestaurantId = restaurant.Id, Number = 2, Capacity = 4, Status = TableStatus.AVAILABLE, PositionX = 250, PositionY = 100, Width = 80, Height = 80, Shape = TableShape.SQUARE },
            new Table { RestaurantId = restaurant.Id, Number = 3, Capacity = 2, Status = TableStatus.AVAILABLE, PositionX = 400, PositionY = 100, Width = 60, Height = 60, Shape = TableShape.CIRCLE },
            new Table { RestaurantId = restaurant.Id, Number = 4, Capacity = 6, Status = TableStatus.AVAILABLE, PositionX = 100, PositionY = 250, Width = 120, Height = 80, Shape = TableShape.RECTANGLE },
            new Table { RestaurantId = restaurant.Id, Number = 5, Capacity = 4, Status = TableStatus.AVAILABLE, PositionX = 280, PositionY = 250, Width = 80, Height = 80, Shape = TableShape.SQUARE },
            new Table { RestaurantId = restaurant.Id, Number = 6, Capacity = 2, Status = TableStatus.AVAILABLE, PositionX = 420, PositionY = 250, Width = 60, Height = 60, Shape = TableShape.CIRCLE },
            new Table { RestaurantId = restaurant.Id, Number = 7, Capacity = 8, Status = TableStatus.AVAILABLE, PositionX = 100, PositionY = 400, Width = 150, Height = 100, Shape = TableShape.OVAL },
            new Table { RestaurantId = restaurant.Id, Number = 8, Capacity = 4, Status = TableStatus.AVAILABLE, PositionX = 300, PositionY = 400, Width = 80, Height = 80, Shape = TableShape.SQUARE },
            new Table { RestaurantId = restaurant.Id, Number = 9, Capacity = 2, Status = TableStatus.AVAILABLE, PositionX = 100, PositionY = 550, Width = 60, Height = 60, Shape = TableShape.CIRCLE },
            new Table { RestaurantId = restaurant.Id, Number = 10, Capacity = 4, Status = TableStatus.AVAILABLE, PositionX = 250, PositionY = 550, Width = 80, Height = 80, Shape = TableShape.SQUARE }
        };
        context.Tables.AddRange(tables);
        await context.SaveChangesAsync();

        // Crear 5 Categorías
        var categories = new List<Category>
        {
            new Category { RestaurantId = restaurant.Id, Name = "Platos Típicos", Description = "Comida tradicional salvadoreña", IsActive = true },
            new Category { RestaurantId = restaurant.Id, Name = "Bebidas", Description = "Refrescos y bebidas naturales", IsActive = true },
            new Category { RestaurantId = restaurant.Id, Name = "Sopas", Description = "Sopas tradicionales", IsActive = true },
            new Category { RestaurantId = restaurant.Id, Name = "Postres", Description = "Dulces típicos", IsActive = true },
            new Category { RestaurantId = restaurant.Id, Name = "Antojitos", Description = "Bocadillos y aperitivos", IsActive = true }
        };
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        // Crear 20 Productos
        var products = new List<Product>
        {
            // Platos Típicos
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[0].Id, Name = "Pupusas Revueltas", Description = "Pupusas de queso con chicharrón", Price = 0.75m, IsAvailable = true },
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[0].Id, Name = "Pupusas de Queso", Description = "Pupusas con queso", Price = 0.65m, IsAvailable = true },
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[0].Id, Name = "Yuca Frita", Description = "Yuca con chicharrón", Price = 4.50m, IsAvailable = true },
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[0].Id, Name = "Panes con Pollo", Description = "Pan con pollo y encurtido", Price = 3.50m, IsAvailable = true },
            
            // Bebidas
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[1].Id, Name = "Horchata", Description = "Bebida de morro", Price = 1.50m, IsAvailable = true },
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[1].Id, Name = "Fresco de Ensalada", Description = "Bebida de frutas", Price = 1.50m, IsAvailable = true },
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[1].Id, Name = "Tamarindo", Description = "Refresco de tamarindo", Price = 1.25m, IsAvailable = true },
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[1].Id, Name = "Coca Cola", Description = "Gaseosa 12oz", Price = 1.00m, IsAvailable = true },
            
            // Sopas
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[2].Id, Name = "Sopa de Res", Description = "Sopa con verduras", Price = 6.00m, IsAvailable = true },
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[2].Id, Name = "Sopa de Gallina India", Description = "Sopa tradicional", Price = 7.00m, IsAvailable = true },
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[2].Id, Name = "Sopa de Pata", Description = "Sopa con pata de res", Price = 6.50m, IsAvailable = true },
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[2].Id, Name = "Mondongo", Description = "Sopa de mondongo", Price = 6.50m, IsAvailable = true },
            
            // Postres
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[3].Id, Name = "Atol de Elote", Description = "Atol caliente", Price = 1.50m, IsAvailable = true },
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[3].Id, Name = "Nuegados", Description = "Dulce de yuca", Price = 2.00m, IsAvailable = true },
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[3].Id, Name = "Empanadas de Plátano", Description = "Con leche", Price = 1.75m, IsAvailable = true },
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[3].Id, Name = "Tres Leches", Description = "Pastel tres leches", Price = 3.00m, IsAvailable = true },
            
            // Antojitos
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[4].Id, Name = "Tamales de Elote", Description = "Tamales dulces", Price = 1.25m, IsAvailable = true },
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[4].Id, Name = "Riguas", Description = "Tortillas de elote", Price = 1.00m, IsAvailable = true },
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[4].Id, Name = "Elotes Locos", Description = "Elotes con mayonesa", Price = 2.00m, IsAvailable = true },
            new Product { RestaurantId = restaurant.Id, CategoryId = categories[4].Id, Name = "Pastelitos", Description = "Pastelitos de carne", Price = 0.75m, IsAvailable = true }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // Crear algunas reservaciones de ejemplo
        var reservations = new List<Reservation>
        {
            new Reservation
            {
                RestaurantId = restaurant.Id,
                TableId = tables[0].Id,
                CustomerName = "Roberto García",
                CustomerPhone = "+503 7123-4567",
                CustomerEmail = "roberto@email.com",
                NumberOfPeople = 4,
                ReservationDate = DateTime.Today.AddDays(1),
                StartTime = new TimeSpan(19, 0, 0),
                EndTime = new TimeSpan(21, 0, 0),
                Status = ReservationStatus.CONFIRMED,
                Source = ReservationSource.WEB
            },
            new Reservation
            {
                RestaurantId = restaurant.Id,
                TableId = tables[3].Id,
                CustomerName = "Carmen López",
                CustomerPhone = "+503 7234-5678",
                NumberOfPeople = 6,
                ReservationDate = DateTime.Today,
                StartTime = new TimeSpan(18, 30, 0),
                EndTime = new TimeSpan(20, 30, 0),
                Status = ReservationStatus.PENDING,
                Source = ReservationSource.WHATSAPP
            }
        };
        context.Reservations.AddRange(reservations);
        await context.SaveChangesAsync();

        // Crear una promoción
        var promotion = new Promotion
        {
            RestaurantId = restaurant.Id,
            Title = "2x1 en Pupusas los Martes",
            Description = "Todos los martes 2x1 en pupusas de cualquier tipo",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(3),
            IsActive = true
        };
        context.Promotions.Add(promotion);
        await context.SaveChangesAsync();
    }
}
