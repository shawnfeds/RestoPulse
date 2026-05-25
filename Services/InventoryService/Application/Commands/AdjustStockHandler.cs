using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.InventoryService.Contracts;
using RestoPulse.InventoryService.Domain.Enums;
using RestoPulse.InventoryService.Infrastructure.Persistence;

namespace RestoPulse.InventoryService.Application.Commands;

public class AdjustStockHandler(InventoryDbContext db, IPublishEndpoint bus)
    : IRequestHandler<AdjustStockCommand, InventoryItemResponse?>
{
    public async Task<InventoryItemResponse?> Handle(
        AdjustStockCommand cmd, CancellationToken ct)
    {
        var item = await db.InventoryItems
            .FirstOrDefaultAsync(i => i.Id == cmd.Id, ct);

        if (item is null) return null;

        if (!Enum.TryParse<AdjustmentType>(cmd.Type, out var type))
            throw new ArgumentException($"Invalid adjustment type: {cmd.Type}");

        item.Adjust(type, cmd.Quantity, cmd.Source, cmd.Reason, cmd.ReferenceNo);
        await db.SaveChangesAsync(ct);

        // Publish LowStockAlert if threshold breached
        foreach (var evt in item.Events)
            await bus.Publish(evt, ct);
        item.ClearEvents();

        return new InventoryItemResponse(
            item.Id, item.MenuItemId, item.Name, item.Unit,
            item.CurrentStock, item.MinThreshold, item.CostPerUnit,
            item.IsLowStock, item.LastUpdated);
    }
}