namespace RestoPulse.InventoryService.Domain.Events;

public abstract record InventoryIntegrationEvent(DateTime OccurredAt)
{
    protected InventoryIntegrationEvent() : this(DateTime.UtcNow) { }
}

public record LowStockAlertEvent(
    int InventoryItemId,
    string ItemName,
    decimal CurrentStock,
    decimal MinThreshold,
    string Unit,
    DateTime OccurredAt) : InventoryIntegrationEvent(OccurredAt)
{
    public LowStockAlertEvent(int id, string name,
        decimal current, decimal min, string unit)
        : this(id, name, current, min, unit, DateTime.UtcNow) { }
}