using System;

namespace RestoPulse.Contracts;

public abstract record KitchenIntegrationEvent(DateTime OccurredAt)
{
    protected KitchenIntegrationEvent() : this(DateTime.UtcNow) { }
}

// Published when a kitchen ticket is bumped (marked ready/done)
public record DishReadyEvent(
    int TicketId,
    string OrderNo,
    int TableNo,
    string ItemName,
    int Qty,
    DateTime OccurredAt) : KitchenIntegrationEvent(OccurredAt)
{
    public DishReadyEvent(int ticketId, string orderNo, int tableNo, string itemName, int qty)
        : this(ticketId, orderNo, tableNo, itemName, qty, DateTime.UtcNow) { }
}
