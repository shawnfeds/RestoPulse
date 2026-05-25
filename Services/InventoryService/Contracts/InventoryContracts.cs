namespace RestoPulse.InventoryService.Contracts;

public record InventoryItemResponse(
    int Id,
    int MenuItemId,
    string Name,
    string Unit,
    decimal CurrentStock,
    decimal MinThreshold,
    decimal CostPerUnit,
    bool IsLowStock,
    DateTime LastUpdated);

public record StockAdjustmentResponse(
    int Id,
    string Type,
    decimal Quantity,
    decimal StockBefore,
    decimal StockAfter,
    string Source,
    string? Reason,
    string? ReferenceNo,
    DateTime CreatedAt);

public record CreateInventoryItemRequest(
    int MenuItemId,
    string Name,
    string Unit,
    decimal InitialStock,
    decimal MinThreshold,
    decimal CostPerUnit);

public record AdjustStockRequest(
    string Type,
    decimal Quantity,
    string? Reason);