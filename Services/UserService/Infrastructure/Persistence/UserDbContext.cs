using Microsoft.EntityFrameworkCore;
using RestoPulse.UserService.Domain.Entities;
using RestoPulse.UserService.Domain.Enums;

namespace RestoPulse.UserService.Infrastructure.Persistence;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Username).IsRequired().HasMaxLength(50);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(100);
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(200);
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.Username).IsUnique();

            // Seed four test users with BCrypt-hashed passwords
            // BCrypt hash of "password" — pre-computed so migrations are deterministic
            const string hash = "$2a$11$K7VqVe1ROIgjMJFxJhBRWOdDlOuMPTqnWj9TO6W3qPwlAxqvL7dHe";

            e.HasData(
                new { Id = 1, Username = "owner",   FullName = "Admin Owner",  PasswordHash = hash, Role = UserRole.Owner,   CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = 2, Username = "manager", FullName = "Jane Manager", PasswordHash = hash, Role = UserRole.Manager, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = 3, Username = "chef",    FullName = "Chef Pierre",  PasswordHash = hash, Role = UserRole.Chef,    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = 4, Username = "server",  FullName = "Sam Server",   PasswordHash = hash, Role = UserRole.Server,  CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        });
    }
}
