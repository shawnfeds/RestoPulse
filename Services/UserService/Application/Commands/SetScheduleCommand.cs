using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.UserService.Domain.Entities;
using RestoPulse.UserService.Infrastructure.Persistence;

namespace RestoPulse.UserService.Application.Commands;

public record SetScheduleCommand(int UserId, DateOnly Date, int ShiftTypeId) : IRequest<bool>;

public class SetScheduleHandler(UserDbContext db) : IRequestHandler<SetScheduleCommand, bool>
{
    public async Task<bool> Handle(SetScheduleCommand cmd, CancellationToken ct)
    {
        var userExists = await db.Users.AnyAsync(u => u.Id == cmd.UserId && u.IsActive, ct);
        var shiftTypeExists = await db.ShiftTypes.AnyAsync(s => s.Id == cmd.ShiftTypeId, ct);

        if (!userExists || !shiftTypeExists)
            return false;

        // Upsert logic
        var existing = await db.UserSchedules
            .FirstOrDefaultAsync(s => s.UserId == cmd.UserId && s.Date == cmd.Date, ct);

        if (existing != null)
        {
            // Update
            db.UserSchedules.Remove(existing);
            await db.SaveChangesAsync(ct);
        }

        var newSchedule = UserSchedule.Create(cmd.UserId, cmd.Date, cmd.ShiftTypeId);
        db.UserSchedules.Add(newSchedule);

        await db.SaveChangesAsync(ct);
        return true;
    }
}
