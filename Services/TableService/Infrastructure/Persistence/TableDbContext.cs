using Microsoft.EntityFrameworkCore;
using RestoPulse.TableService.Domain.Entities;
using RestoPulse.TableService.Domain.Enums;
using System.Reflection.Emit;

namespace RestoPulse.TableService.Infrastructure.Persistence;

public class TableDbContext(DbContextOptions<TableDbContext> options) : DbContext(options)
{
    public DbSet<Table> Tables => Set<Table>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Table>(e =>
        {
            e.ToTable("Tables");
            e.HasKey(x => x.Id);
            e.Property(x => x.TableNo).IsRequired();
            e.Property(x => x.Section).IsRequired().HasMaxLength(100);
            e.Property(x => x.AssignedStaff).HasMaxLength(100);
            e.Property(x => x.CurrentOrderId).HasMaxLength(50);
            e.Property(x => x.Status)
             .HasConversion<string>()   // store as string not int
             .HasMaxLength(20);

            e.HasIndex(x => x.TableNo).IsUnique();

            e.Ignore(x => x.DomainEvents); // not persisted
        });
    }
}