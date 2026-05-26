namespace RestoPulse.ReportService.Domain.Entities;

public class Revenue
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string BillNo { get; private set; } = string.Empty;
    public string OrderNo { get; private set; } = string.Empty;
    public int TableId { get; private set; }
    public int TableNo { get; private set; }
    public decimal Amount { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public DateTime SettledAt { get; private set; }

    private Revenue() { }

    public static Revenue Create(
        string billNo, string orderNo,
        int tableId, int tableNo,
        decimal amount, string paymentMethod,
        DateTime settledAt) => new()
        {
            BillNo = billNo,
            OrderNo = orderNo,
            TableId = tableId,
            TableNo = tableNo,
            Amount = amount,
            PaymentMethod = paymentMethod,
            SettledAt = settledAt
        };
}