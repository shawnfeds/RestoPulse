using MediatR;
using RestoPulse.UserService.Contracts;
using RestoPulse.UserService.Infrastructure.Persistence;

namespace RestoPulse.UserService.Application.Commands;

public record UpdateUserCommand(int Id, string FullName, string Role) : IRequest<UserResponse?>;

public class UpdateUserHandler(UserDbContext db) : IRequestHandler<UpdateUserCommand, UserResponse?>
{
    public async Task<UserResponse?> Handle(UpdateUserCommand cmd, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([cmd.Id], ct);
        if (user == null) return null;

        user.Update(cmd.FullName, cmd.Role);
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
}
