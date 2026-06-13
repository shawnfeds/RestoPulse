using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.InventoryService.Contracts;
using RestoPulse.InventoryService.Domain.Enums;
using RestoPulse.InventoryService.Infrastructure.Persistence;

namespace RestoPulse.InventoryService.Application.Queries;

public record GetInventoryUsageQuery(int Month, int Year) : IRequest<List<InventoryUsageResponse>>;

public class GetInventoryUsageHandler(InventoryDbContext db)
    : IRequestHandler<GetInventoryUsageQuery, List<InventoryUsageResponse>>
{
    public async Task<List<InventoryUsageResponse>> Handle(
        GetInventoryUsageQuery request, CancellationToken ct)
    {
        var from = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to   = from.AddMonths(1);

        var results = await db.Adjustments
            .Where(a => a.CreatedAt >= from && a.CreatedAt < to && a.Type == AdjustmentType.Deduction)
            .Join(db.InventoryItems,
                  adj  => adj.InventoryItemId,
                  item => item.Id,
                  (adj, item) => new { adj, item })
            .GroupBy(x => new { x.item.Id, x.item.Name, x.item.Unit, x.item.CostPerUnit })
            .Select(g => new
            {
                g.Key.Id,
                g.Key.Name,
                g.Key.Unit,
                g.Key.CostPerUnit,
                TotalUsed  = g.Sum(x => x.adj.Quantity),
                UsageCount = g.Count()
            })
            .OrderByDescending(x => x.TotalUsed)
            .ToListAsync(ct);

        return results.Select((x, idx) => new InventoryUsageResponse(
            Rank: idx + 1,
            ItemId: x.Id,
            Name: x.Name,
            Unit: x.Unit,
            TotalUsed: x.TotalUsed,
            TotalCost: Math.Round(x.TotalUsed * x.CostPerUnit, 2),
            UsageCount: x.UsageCount
        )).ToList();
    }
}
