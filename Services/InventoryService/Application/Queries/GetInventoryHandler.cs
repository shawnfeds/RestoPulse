using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.InventoryService.Contracts;
using RestoPulse.InventoryService.Infrastructure.Persistence;

namespace RestoPulse.InventoryService.Application.Queries;

public class GetInventoryHandler(InventoryDbContext db)
    : IRequestHandler<GetInventoryQuery, List<InventoryItemResponse>>
{
    public async Task<List<InventoryItemResponse>> Handle(
        GetInventoryQuery request, CancellationToken ct)
    {
        var query = db.InventoryItems.AsQueryable();

        if (request.LowStockOnly)
            query = query.Where(i => i.CurrentStock <= i.MinThreshold);

        return await query
            .OrderBy(i => i.Name)
            .Select(i => new InventoryItemResponse(
                i.Id, i.MenuItemId, i.Name, i.Unit,
                i.CurrentStock, i.MinThreshold, i.CostPerUnit,
                i.CurrentStock <= i.MinThreshold,
                i.LastUpdated))
            .ToListAsync(ct);
    }
}