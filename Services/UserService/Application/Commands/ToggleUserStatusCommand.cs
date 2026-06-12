using MediatR;
using RestoPulse.UserService.Infrastructure.Persistence;

namespace RestoPulse.UserService.Application.Commands;

public record ToggleUserStatusCommand(int Id, bool IsActive) : IRequest<bool>;

public class ToggleUserStatusHandler(UserDbContext db) : IRequestHandler<ToggleUserStatusCommand, bool>
{
    public async Task<bool> Handle(ToggleUserStatusCommand cmd, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([cmd.Id], ct);
        if (user == null) return false;

        user.SetStatus(cmd.IsActive);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
