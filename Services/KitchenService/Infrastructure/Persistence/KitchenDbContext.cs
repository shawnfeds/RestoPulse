using Microsoft.EntityFrameworkCore;
using RestoPulse.KitchenService.Domain.Entities;

namespace RestoPulse.KitchenService.Infrastructure.Persistence;

public class KitchenDbContext(DbContextOptions<KitchenDbContext> options) : DbContext(options)
{
    public DbSet<KitchenTicket> Tickets => Set<KitchenTicket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KitchenTicket>(e =>
        {
            e.ToTable("KitchenTickets");
            e.HasKey(x => x.Id);
            e.Property(x => x.TicketNo).IsRequired().HasMaxLength(20);
            e.Property(x => x.OrderNo).IsRequired().HasMaxLength(30);
            e.Property(x => x.ItemName).IsRequired().HasMaxLength(200);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.Category).HasMaxLength(50);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
        });
    }
}