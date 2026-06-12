using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.UserService.Contracts;
using RestoPulse.UserService.Infrastructure.Persistence;

namespace RestoPulse.UserService.Application.Queries;

// 1. Get Shifts Logs
public record GetShiftsQuery(int? UserId, DateOnly? Date) : IRequest<List<ShiftResponse>>;

public class GetShiftsHandler(UserDbContext db) : IRequestHandler<GetShiftsQuery, List<ShiftResponse>>
{
    public async Task<List<ShiftResponse>> Handle(GetShiftsQuery query, CancellationToken ct)
    {
        var dbQuery = db.Shifts
            .Include(s => s.User)
            .Include(s => s.ScheduledShiftType)
            .AsQueryable();

        if (query.UserId.HasValue)
            dbQuery = dbQuery.Where(s => s.UserId == query.UserId.Value);

        if (query.Date.HasValue)
            dbQuery = dbQuery.Where(s => s.Date == query.Date.Value);

        var list = await dbQuery
            .OrderByDescending(s => s.ClockInTime)
            .ToListAsync(ct);

        return list.Select(s => new ShiftResponse(
            s.Id,
            s.UserId,
            s.User.FullName,
            s.ClockInTime,
            s.ClockOutTime,
            s.IsLate,
            s.OvertimeMinutes,
            s.RegularMinutes,
            s.Status,
            s.Date,
            s.ScheduledShiftType?.Name,
            s.Notes
        )).ToList();
    }
}

// 2. Get User Schedules for a Date
public record GetUserSchedulesQuery(DateOnly Date) : IRequest<List<UserScheduleResponse>>;

public class GetUserSchedulesHandler(UserDbContext db) : IRequestHandler<GetUserSchedulesQuery, List<UserScheduleResponse>>
{
    public async Task<List<UserScheduleResponse>> Handle(GetUserSchedulesQuery query, CancellationToken ct)
    {
        var list = await db.UserSchedules
            .Include(s => s.User)
            .Include(s => s.ShiftType)
            .Where(s => s.Date == query.Date)
            .ToListAsync(ct);

        return list.Select(s => new UserScheduleResponse(
            s.Id,
            s.UserId,
            s.User.FullName,
            s.Date,
            s.ShiftTypeId,
            s.ShiftType.Name
        )).ToList();
    }
}
