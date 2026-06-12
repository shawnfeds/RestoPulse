using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.UserService.Contracts;
using RestoPulse.UserService.Domain.Entities;
using RestoPulse.UserService.Infrastructure.Persistence;
using System.Security.Cryptography;
using System.Text;

namespace RestoPulse.UserService.Application.Commands;

public record CreateUserCommand(string Username, string Password, string FullName, string Role) : IRequest<UserResponse?>;

public class CreateUserHandler(UserDbContext db) : IRequestHandler<CreateUserCommand, UserResponse?>
{
    public async Task<UserResponse?> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        var username = cmd.Username.ToLowerInvariant().Trim();
        var exists = await db.Users.AnyAsync(u => u.Username == username, ct);
        if (exists) return null; // Conflict

        var passwordHash = HashPassword(cmd.Password);
        var user = User.Create(cmd.Username, passwordHash, cmd.FullName, cmd.Role);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return new UserResponse(
            user.Id,
            user.Username,
            user.FullName,
            user.Role,
            user.IsActive,
            user.CreatedAt
        );
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
