namespace RestoPulse.Contracts;

public abstract record BillIntegrationEvent(DateTime OccurredAt)
{
    protected BillIntegrationEvent() : this(DateTime.UtcNow) { }
}

public record BillSettledEvent(
    string BillNo,
    string OrderNo,
    int TableId,
    int TableNo,
    decimal Total,
    string PaymentMethod,
    DateTime OccurredAt) : BillIntegrationEvent(OccurredAt)
{
    public BillSettledEvent(string billNo, string orderNo,
        int tableId, int tableNo, decimal total, string paymentMethod)
        : this(billNo, orderNo, tableId, tableNo, total, paymentMethod, DateTime.UtcNow) { }
}
