using Microsoft.EntityFrameworkCore;
using RestoPulse.ReportService.Domain.Entities;

namespace RestoPulse.ReportService.Infrastructure.Persistence;

public class ReportDbContext(DbContextOptions<ReportDbContext> options) : DbContext(options)
{
    public DbSet<Revenue> Revenues => Set<Revenue>();
    public DbSet<ItemSale> ItemSales => Set<ItemSale>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Revenue>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.BillNo).HasMaxLength(50);
            e.Property(x => x.OrderNo).HasMaxLength(50);
            e.Property(x => x.PaymentMethod).HasMaxLength(50);
            e.HasIndex(x => x.BillNo).IsUnique();
            e.HasIndex(x => x.OrderNo);
            e.HasIndex(x => x.SettledAt);
        });

        modelBuilder.Entity<ItemSale>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.OrderNo).HasMaxLength(50);
            e.Property(x => x.ItemName).HasMaxLength(200);
            e.HasIndex(x => x.OrderNo);
            e.HasIndex(x => x.MenuItemId);
            e.HasIndex(x => x.OrderedAt);
        });
    }
}