using Microsoft.EntityFrameworkCore;
using RestaurantFlow.Domain.Entities;

namespace RestaurantFlow.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Table> Tables { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Promotion> Promotions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Restaurant
        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasIndex(e => e.Email);
            
            entity.HasOne(e => e.Restaurant)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Table
        modelBuilder.Entity<Table>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).IsRequired();
            
            entity.HasOne(e => e.Restaurant)
                .WithMany(r => r.Tables)
                .HasForeignKey(e => e.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            
            entity.HasOne(e => e.Restaurant)
                .WithMany(r => r.Categories)
                .HasForeignKey(e => e.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.Restaurant)
                .WithMany(r => r.Products)
                .HasForeignKey(e => e.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Tax).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.Restaurant)
                .WithMany(r => r.Orders)
                .HasForeignKey(e => e.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Table)
                .WithMany(t => t.Orders)
                .HasForeignKey(e => e.TableId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Waiter)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.WaiterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Cashier)
                .WithMany(u => u.Payments)
                .HasForeignKey(e => e.CashierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Reservation
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CustomerPhone).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CustomerEmail).HasMaxLength(200);
            
            entity.HasOne(e => e.Restaurant)
                .WithMany(r => r.Reservations)
                .HasForeignKey(e => e.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(e => e.TableId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Promotion
        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            
            entity.HasOne(e => e.Restaurant)
                .WithMany(r => r.Promotions)
                .HasForeignKey(e => e.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
