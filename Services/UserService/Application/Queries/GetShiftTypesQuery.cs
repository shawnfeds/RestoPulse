using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.UserService.Contracts;
using RestoPulse.UserService.Infrastructure.Persistence;

namespace RestoPulse.UserService.Application.Queries;

public record GetShiftTypesQuery : IRequest<List<ShiftTypeResponse>>;

public class GetShiftTypesHandler(UserDbContext db) : IRequestHandler<GetShiftTypesQuery, List<ShiftTypeResponse>>
{
    public async Task<List<ShiftTypeResponse>> Handle(GetShiftTypesQuery query, CancellationToken ct)
    {
        return await db.ShiftTypes
            .OrderBy(s => s.StartTime)
            .Select(s => new ShiftTypeResponse(
                s.Id,
                s.Name,
                s.StartTime.ToString(@"hh\:mm"),
                s.EndTime.ToString(@"hh\:mm")
            ))
            .ToListAsync(ct);
    }
}
