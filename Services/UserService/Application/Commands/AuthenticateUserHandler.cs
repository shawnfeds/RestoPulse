using Isopoh.Cryptography.Argon2;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.UserService.Contracts;
using RestoPulse.UserService.Infrastructure.Persistence;

namespace RestoPulse.UserService.Application.Commands;

public class AuthenticateUserHandler(UserDbContext db)
    : IRequestHandler<AuthenticateUserCommand, LoginResponse?>
{
    public async Task<LoginResponse?> Handle(
        AuthenticateUserCommand cmd, CancellationToken ct)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Username == cmd.Username, ct);

        if (user is null)
            return null;

        if (!Argon2.Verify(user.PasswordHash, cmd.Password))
            return null;

        // Generate a simple token (role + username encoded for dev use)
        var token = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{user.Username}:{user.Role}:{user.Id}"));

        return new LoginResponse(
            token,
            user.Username,
            user.FullName,
            user.Role.ToString());
    }
}
