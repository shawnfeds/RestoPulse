namespace RestoPulse.Contracts;

// Base marker
public abstract record OrderIntegrationEvent(DateTime OccurredAt)
{
    protected OrderIntegrationEvent() : this(DateTime.UtcNow) { }
}

// Published when order is first created — Kitchen subscribes
public record OrderCreatedEvent(
    string OrderNo,
    int TableId,
    int TableNo,
    string StaffName,
    DateTime OccurredAt) : OrderIntegrationEvent(OccurredAt)
{
    public OrderCreatedEvent(string orderNo, int tableId, int tableNo, string staffName)
        : this(orderNo, tableId, tableNo, staffName, DateTime.UtcNow) { }

    // Items added after creation — set by handler before publishing
    public List<OrderCreatedEventItem> Items { get; init; } = [];
}

public record OrderCreatedEventItem(
    int MenuItemId,
    string Name,
    decimal Price,
    int Qty,
    string? Notes);

// Published on every status change — Billing + Inventory subscribe
public record OrderStatusChangedEvent(
    string OrderNo,
    int TableId,
    int TableNo,
    string NewStatus,
    DateTime OccurredAt) : OrderIntegrationEvent(OccurredAt)
{
    public OrderStatusChangedEvent(string orderNo, int tableId, int tableNo, string status)
        : this(orderNo, tableId, tableNo, status, DateTime.UtcNow) { }
}
