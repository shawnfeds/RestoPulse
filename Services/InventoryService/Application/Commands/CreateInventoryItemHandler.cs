using MediatR;
using RestoPulse.InventoryService.Contracts;
using RestoPulse.InventoryService.Domain.Entities;
using RestoPulse.InventoryService.Infrastructure.Persistence;

namespace RestoPulse.InventoryService.Application.Commands;

public class CreateInventoryItemHandler(InventoryDbContext db)
    : IRequestHandler<CreateInventoryItemCommand, InventoryItemResponse>
{
    public async Task<InventoryItemResponse> Handle(
        CreateInventoryItemCommand cmd, CancellationToken ct)
    {
        var item = InventoryItem.Create(
            cmd.MenuItemId, cmd.Name, cmd.Unit,
            cmd.InitialStock, cmd.MinThreshold, cmd.CostPerUnit);

        db.InventoryItems.Add(item);
        await db.SaveChangesAsync(ct);

        return new InventoryItemResponse(
            item.Id, item.MenuItemId, item.Name, item.Unit,
            item.CurrentStock, item.MinThreshold, item.CostPerUnit,
            item.IsLowStock, item.LastUpdated);
    }
}