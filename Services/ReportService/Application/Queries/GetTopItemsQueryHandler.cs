using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.ReportService.Infrastructure.Persistence;

namespace RestoPulse.ReportService.Application.Queries;

public class GetTopItemsQueryHandler(ReportDbContext db)
    : IRequestHandler<GetTopItemsQuery, IReadOnlyList<TopItemDto>>
{
    public async Task<IReadOnlyList<TopItemDto>> Handle(GetTopItemsQuery request, CancellationToken ct)
    {
        var results = await db.ItemSales
            .Where(r => r.OrderedAt >= request.From && r.OrderedAt <= request.To)
            .GroupBy(r => new { r.MenuItemId, r.ItemName })
            .Select(g => new TopItemDto(
                g.Key.MenuItemId,
                g.Key.ItemName,
                g.Sum(x => x.Quantity),
                g.Sum(x => x.Quantity * x.UnitPrice)))
            .OrderByDescending(x => x.TotalQuantity)
            .Take(request.Limit)
            .ToListAsync(ct);

        return results;
    }
}