using RestoPulse.InventoryService.Domain.Enums;
using RestoPulse.InventoryService.Domain.Events;

namespace RestoPulse.InventoryService.Domain.Entities;

public class InventoryItem
{
    public int Id { get; private set; }
    public int MenuItemId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public decimal CurrentStock { get; private set; }
    public decimal MinThreshold { get; private set; }
    public decimal CostPerUnit { get; private set; }
    public DateTime LastUpdated { get; private set; }

    private readonly List<StockAdjustment> _adjustments = [];
    public IReadOnlyList<StockAdjustment> Adjustments => _adjustments.AsReadOnly();

    private readonly List<InventoryIntegrationEvent> _events = [];
    public IReadOnlyList<InventoryIntegrationEvent> Events => _events.AsReadOnly();

    private InventoryItem() { }

    public static InventoryItem Create(int menuItemId, string name,
        string unit, decimal initialStock, decimal minThreshold, decimal costPerUnit)
    {
        return new InventoryItem
        {
            MenuItemId = menuItemId,
            Name = name,
            Unit = unit,
            CurrentStock = initialStock,
            MinThreshold = minThreshold,
            CostPerUnit = costPerUnit,
            LastUpdated = DateTime.UtcNow
        };
    }

    public StockAdjustment Adjust(AdjustmentType type, decimal quantity,
        string source, string? reason = null, string? referenceNo = null)
    {
        var before = CurrentStock;

        CurrentStock = type switch
        {
            AdjustmentType.Addition => CurrentStock + quantity,
            AdjustmentType.Deduction => CurrentStock - quantity,
            AdjustmentType.Correction => quantity,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (CurrentStock < 0) CurrentStock = 0;
        LastUpdated = DateTime.UtcNow;

        var adjustment = StockAdjustment.Create(
            Id, type, quantity, before, CurrentStock, source, reason, referenceNo);
        _adjustments.Add(adjustment);

        if (CurrentStock <= MinThreshold)
            _events.Add(new LowStockAlertEvent(Id, Name, CurrentStock, MinThreshold, Unit));

        return adjustment;
    }

    public bool IsLowStock => CurrentStock <= MinThreshold;

    public void ClearEvents() => _events.Clear();
}