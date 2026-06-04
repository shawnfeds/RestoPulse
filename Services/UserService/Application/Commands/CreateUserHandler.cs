using Isopoh.Cryptography.Argon2;
using MediatR;
using RestoPulse.UserService.Contracts;
using RestoPulse.UserService.Domain.Entities;
using RestoPulse.UserService.Domain.Enums;
using RestoPulse.UserService.Infrastructure.Persistence;

namespace RestoPulse.UserService.Application.Commands;

public class CreateUserHandler(UserDbContext db)
    : IRequestHandler<CreateUserCommand, UserResponse>
{
    public async Task<UserResponse> Handle(
        CreateUserCommand cmd, CancellationToken ct)
    {
        if (!Enum.TryParse<UserRole>(cmd.Role, out var role))
            throw new InvalidOperationException($"Invalid role: {cmd.Role}");

        var passwordHash = Argon2.Hash(cmd.Password);

        var user = User.Create(cmd.Username, cmd.FullName, passwordHash, role);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return new UserResponse(
            user.Id,
            user.Username,
            user.FullName,
            user.Role.ToString(),
            user.CreatedAt);
    }
}
