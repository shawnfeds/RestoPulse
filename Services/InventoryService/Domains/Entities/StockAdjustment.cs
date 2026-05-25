using RestoPulse.InventoryService.Domain.Enums;

namespace RestoPulse.InventoryService.Domain.Entities;

public class StockAdjustment
{
    public int Id { get; private set; }
    public int InventoryItemId { get; private set; }
    public AdjustmentType Type { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal StockBefore { get; private set; }
    public decimal StockAfter { get; private set; }
    public string? Reason { get; private set; }
    public string Source { get; private set; } = string.Empty; // "Manual" | "OrderDeduction" | "System"
    public string? ReferenceNo { get; private set; } // OrderNo if auto-deducted
    public DateTime CreatedAt { get; private set; }

    private StockAdjustment() { }

    public static StockAdjustment Create(int inventoryItemId, AdjustmentType type,
        decimal quantity, decimal before, decimal after,
        string source, string? reason = null, string? referenceNo = null)
    {
        return new StockAdjustment
        {
            InventoryItemId = inventoryItemId,
            Type = type,
            Quantity = quantity,
            StockBefore = before,
            StockAfter = after,
            Source = source,
            Reason = reason,
            ReferenceNo = referenceNo,
            CreatedAt = DateTime.UtcNow
        };
    }
}