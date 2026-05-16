using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.TableService.Contracts;
using RestoPulse.TableService.Domain.Enums;
using RestoPulse.TableService.Infrastructure.Persistence;

namespace RestoPulse.TableService.Application.Queries;

public class GetTablesHandler(TableDbContext db)
    : IRequestHandler<GetTablesQuery, List<TableResponse>>
{
    public async Task<List<TableResponse>> Handle(
        GetTablesQuery request, CancellationToken ct)
    {
        var query = db.Tables.AsQueryable();

        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<TableStatus>(request.Status, out var status))
            query = query.Where(t => t.Status == status);

        return await query
            .OrderBy(t => t.TableNo)
            .Select(t => new TableResponse(
                t.Id, t.TableNo, t.Capacity, t.Section,
                t.Status.ToString(), t.CurrentOrderId, t.AssignedStaff))
            .ToListAsync(ct);
    }
}