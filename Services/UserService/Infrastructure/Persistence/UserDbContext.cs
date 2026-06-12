using Microsoft.EntityFrameworkCore;
using RestoPulse.UserService.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace RestoPulse.UserService.Infrastructure.Persistence;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ShiftType> ShiftTypes => Set<ShiftType>();
    public DbSet<UserSchedule> UserSchedules => Set<UserSchedule>();
    public DbSet<Shift> Shifts => Set<Shift>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Username).IsRequired().HasMaxLength(50);
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(200);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Role).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<ShiftType>(e =>
        {
            e.ToTable("ShiftTypes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<UserSchedule>(e =>
        {
            e.ToTable("UserSchedules");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.Date }).IsUnique();

            e.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.ShiftType)
             .WithMany()
             .HasForeignKey(x => x.ShiftTypeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Shift>(e =>
        {
            e.ToTable("Shifts");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.ScheduledShiftType)
             .WithMany()
             .HasForeignKey(x => x.ScheduledShiftTypeId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Seed Data ──────────────────────────────────────────────
        SeedShiftTypes(modelBuilder);
        SeedUsers(modelBuilder);
    }

    private static void SeedShiftTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShiftType>().HasData(
            new { Id = 1, Name = "Morning", StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0) },
            new { Id = 2, Name = "Evening", StartTime = new TimeSpan(17, 0, 0), EndTime = new TimeSpan(1, 0, 0) },
            new { Id = 3, Name = "Night", StartTime = new TimeSpan(21, 0, 0), EndTime = new TimeSpan(5, 0, 0) }
        );
    }

    private static void SeedUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new { Id = 1, Username = "owner", PasswordHash = HashPassword("owner123"), FullName = "Anand Dixit (Owner)", Role = "Owner", IsActive = true, CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc) },
            new { Id = 2, Username = "manager", PasswordHash = HashPassword("manager123"), FullName = "Priya Sharma (Manager)", Role = "Manager", IsActive = true, CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc) },
            new { Id = 3, Username = "chef", PasswordHash = HashPassword("chef123"), FullName = "Ranveer Brar (Chef)", Role = "Chef", IsActive = true, CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc) },
            new { Id = 4, Username = "server", PasswordHash = HashPassword("server123"), FullName = "Rahul Mehta (Server)", Role = "Server", IsActive = true, CreatedAt = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc) }
        );
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
