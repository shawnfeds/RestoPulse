using Microsoft.EntityFrameworkCore;
using RestoPulse.NotificationService.Domains.Entities;

namespace RestoPulse.NotificationService.Infrastructure.Persistence;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("Notifications");
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).IsRequired().HasMaxLength(50);
            e.Property(x => x.Title).IsRequired().HasMaxLength(150);
            e.Property(x => x.Message).IsRequired().HasMaxLength(500);
            e.Property(x => x.ForRoles).IsRequired().HasMaxLength(100);
            e.Property(x => x.EntityId).HasMaxLength(50);
        });
    }
}
