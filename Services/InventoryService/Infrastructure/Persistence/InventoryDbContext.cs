using Microsoft.EntityFrameworkCore;
using RestoPulse.InventoryService.Domain.Entities;

namespace RestoPulse.InventoryService.Infrastructure.Persistence;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<StockAdjustment> Adjustments => Set<StockAdjustment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItem>(e =>
        {
            e.ToTable("InventoryItems");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Unit).IsRequired().HasMaxLength(20);
            e.Property(x => x.CurrentStock).HasColumnType("decimal(10,3)");
            e.Property(x => x.MinThreshold).HasColumnType("decimal(10,3)");
            e.Property(x => x.CostPerUnit).HasColumnType("decimal(10,2)");
            e.Ignore(x => x.Events);

            e.HasMany(x => x.Adjustments)
                .WithOne()
                .HasForeignKey(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Navigation(x => x.Adjustments)
             .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<StockAdjustment>(e =>
        {
            e.ToTable("StockAdjustments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Quantity).HasColumnType("decimal(10,3)");
            e.Property(x => x.StockBefore).HasColumnType("decimal(10,3)");
            e.Property(x => x.StockAfter).HasColumnType("decimal(10,3)");
            e.Property(x => x.Reason).HasMaxLength(500);
            e.Property(x => x.Source).HasMaxLength(50);
            e.Property(x => x.ReferenceNo).HasMaxLength(50);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
        });
    }
}