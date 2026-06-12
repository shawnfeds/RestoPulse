using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.UserService.Contracts;
using RestoPulse.UserService.Infrastructure.Persistence;

namespace RestoPulse.UserService.Application.Commands;

public record ClockOutCommand(int UserId) : IRequest<ShiftResponse?>;

public class ClockOutHandler(UserDbContext db) : IRequestHandler<ClockOutCommand, ShiftResponse?>
{
    public async Task<ShiftResponse?> Handle(ClockOutCommand cmd, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([cmd.UserId], ct);
        if (user == null)
            return null;

        var shift = await db.Shifts
            .Include(s => s.ScheduledShiftType)
            .FirstOrDefaultAsync(s => s.UserId == cmd.UserId && s.Status == "Active", ct);

        if (shift == null)
            return null;

        shift.ClockOut(DateTime.UtcNow, shift.ScheduledShiftType);
        await db.SaveChangesAsync(ct);

        return new ShiftResponse(
            shift.Id,
            shift.UserId,
            user.FullName,
            shift.ClockInTime,
            shift.ClockOutTime,
            shift.IsLate,
            shift.OvertimeMinutes,
            shift.RegularMinutes,
            shift.Status,
            shift.Date,
            shift.ScheduledShiftType?.Name,
            shift.Notes
        );
    }
}
