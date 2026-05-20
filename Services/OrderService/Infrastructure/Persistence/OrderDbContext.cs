using Microsoft.EntityFrameworkCore;
using RestoPulse.OrderService.Domain.Entities;
using RestoPulse.OrderService.Domain.Enums;

namespace RestoPulse.OrderService.Infrastructure.Persistence;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasKey(x => x.Id);
            e.Property(x => x.OrderNo).IsRequired().HasMaxLength(30);
            e.Property(x => x.StaffName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Subtotal).HasColumnType("decimal(10,2)");
            e.Property(x => x.Tax).HasColumnType("decimal(10,2)");
            e.Property(x => x.Total).HasColumnType("decimal(10,2)");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.OrderNo).IsUnique();
            e.Ignore(x => x.Events);

            e.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Navigation(x => x.Items)
             .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("OrderItems");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Price).HasColumnType("decimal(10,2)");
            e.Property(x => x.Notes).HasMaxLength(500);
        });
    }
}