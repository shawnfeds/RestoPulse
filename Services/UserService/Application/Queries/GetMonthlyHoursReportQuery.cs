using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.UserService.Contracts;
using RestoPulse.UserService.Infrastructure.Persistence;

namespace RestoPulse.UserService.Application.Queries;

public record GetMonthlyHoursReportQuery(int UserId, int Month, int Year) : IRequest<MonthlyHoursReport?>;

public class GetMonthlyHoursReportHandler(UserDbContext db) : IRequestHandler<GetMonthlyHoursReportQuery, MonthlyHoursReport?>
{
    public async Task<MonthlyHoursReport?> Handle(GetMonthlyHoursReportQuery query, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([query.UserId], ct);
        if (user == null) return null;

        var shifts = await db.Shifts
            .Include(s => s.ScheduledShiftType)
            .Where(s => s.UserId == query.UserId && s.Date.Year == query.Year && s.Date.Month == query.Month)
            .OrderBy(s => s.ClockInTime)
            .ToListAsync(ct);

        int totalMinutesWorked = 0;
        int totalOvertime = 0;
        int totalRegular = 0;
        int lateCount = 0;

        var shiftResponses = new List<ShiftResponse>();

        foreach (var s in shifts)
        {
            if (s.Status == "Completed" && s.ClockOutTime.HasValue)
            {
                var duration = (int)(s.ClockOutTime.Value - s.ClockInTime).TotalMinutes;
                totalMinutesWorked += duration;
                totalOvertime += s.OvertimeMinutes;
                totalRegular += s.RegularMinutes;
            }

            if (s.IsLate)
            {
                lateCount++;
            }

            shiftResponses.Add(new ShiftResponse(
                s.Id,
                s.UserId,
                user.FullName,
                s.ClockInTime,
                s.ClockOutTime,
                s.IsLate,
                s.OvertimeMinutes,
                s.RegularMinutes,
                s.Status,
                s.Date,
                s.ScheduledShiftType?.Name,
                s.Notes
            ));
        }

        return new MonthlyHoursReport(
            user.Id,
            user.FullName,
            totalMinutesWorked,
            totalOvertime,
            totalRegular,
            lateCount,
            shiftResponses
        );
    }
}
