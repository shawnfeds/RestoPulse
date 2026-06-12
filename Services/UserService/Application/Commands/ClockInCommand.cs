using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.UserService.Contracts;
using RestoPulse.UserService.Domain.Entities;
using RestoPulse.UserService.Infrastructure.Persistence;

namespace RestoPulse.UserService.Application.Commands;

public record ClockInCommand(int UserId, string? Notes) : IRequest<ShiftResponse?>;

public class ClockInHandler(UserDbContext db) : IRequestHandler<ClockInCommand, ShiftResponse?>
{
    public async Task<ShiftResponse?> Handle(ClockInCommand cmd, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([cmd.UserId], ct);
        if (user == null || !user.IsActive)
            return null;

        // Check if already clocked in
        var existing = await db.Shifts
            .Include(s => s.ScheduledShiftType)
            .FirstOrDefaultAsync(s => s.UserId == cmd.UserId && s.Status == "Active", ct);

        if (existing != null)
        {
            return new ShiftResponse(
                existing.Id,
                existing.UserId,
                user.FullName,
                existing.ClockInTime,
                existing.ClockOutTime,
                existing.IsLate,
                existing.OvertimeMinutes,
                existing.RegularMinutes,
                existing.Status,
                existing.Date,
                existing.ScheduledShiftType?.Name,
                existing.Notes
            );
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Find today's schedule
        var schedule = await db.UserSchedules
            .Include(s => s.ShiftType)
            .FirstOrDefaultAsync(s => s.UserId == cmd.UserId && s.Date == today, ct);

        var shift = Shift.ClockIn(cmd.UserId, DateTime.UtcNow, schedule?.ShiftType, cmd.Notes);
        db.Shifts.Add(shift);
        await db.SaveChangesAsync(ct);

        // Re-load to get navigation properties if any
        if (shift.ScheduledShiftTypeId.HasValue)
        {
            shift = await db.Shifts
                .Include(s => s.ScheduledShiftType)
                .FirstAsync(s => s.Id == shift.Id, ct);
        }

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
