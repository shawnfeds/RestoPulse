using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.UserService.Contracts;
using RestoPulse.UserService.Infrastructure.Persistence;

namespace RestoPulse.UserService.Application.Queries;

public record GetClockStatusQuery(int UserId) : IRequest<ClockStatusResponse>;

public class GetClockStatusHandler(UserDbContext db) : IRequestHandler<GetClockStatusQuery, ClockStatusResponse>
{
    public async Task<ClockStatusResponse> Handle(GetClockStatusQuery query, CancellationToken ct)
    {
        var activeShift = await db.Shifts
            .Include(s => s.ScheduledShiftType)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == query.UserId && s.Status == "Active", ct);

        if (activeShift == null)
            return new ClockStatusResponse(false, null);

        var shiftResponse = new ShiftResponse(
            activeShift.Id,
            activeShift.UserId,
            activeShift.User.FullName,
            activeShift.ClockInTime,
            activeShift.ClockOutTime,
            activeShift.IsLate,
            activeShift.OvertimeMinutes,
            activeShift.RegularMinutes,
            activeShift.Status,
            activeShift.Date,
            activeShift.ScheduledShiftType?.Name,
            activeShift.Notes
        );

        return new ClockStatusResponse(true, shiftResponse);
    }
}
