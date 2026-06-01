using RestoPulse.BillingService.Domain.Enums;
using RestoPulse.Contracts;

namespace RestoPulse.BillingService.Domain.Entities;

public class Bill
{
    public int Id { get; private set; }
    public string BillNo { get; private set; } = string.Empty;
    public string OrderNo { get; private set; } = string.Empty;
    public int TableId { get; private set; }
    public int TableNo { get; private set; }
    public BillStatus Status { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxableAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal Total { get; private set; }
    public decimal TaxRate { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; }
    public decimal? AmountTendered { get; private set; }
    public decimal? ChangeReturned { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SettledAt { get; private set; }

    private readonly List<BillItem> _items = [];
    public IReadOnlyList<BillItem> Items => _items.AsReadOnly();

    private readonly List<BillIntegrationEvent> _events = [];
    public IReadOnlyList<BillIntegrationEvent> Events => _events.AsReadOnly();

    private Bill() { }

    public static Bill Create(string orderNo, int tableId, int tableNo,
        decimal taxRate = 18m)
    {
        return new Bill
        {
            BillNo = $"BILL-{DateTime.UtcNow:yyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
            OrderNo = orderNo,
            TableId = tableId,
            TableNo = tableNo,
            Status = BillStatus.Pending,
            TaxRate = taxRate,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(int menuItemId, string name, decimal price, int qty)
    {
        _items.Add(BillItem.Create(menuItemId, name, price, qty));
        Recalculate();
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        DiscountAmount = discountAmount;
        Recalculate();
    }

    public void Settle(PaymentMethod method, decimal? amountTendered = null)
    {
        if (Status != BillStatus.Pending)
            throw new InvalidOperationException("Only pending bills can be settled.");

        PaymentMethod = method;
        AmountTendered = amountTendered;
        ChangeReturned = amountTendered.HasValue
            ? Math.Max(amountTendered.Value - Total, 0)
            : null;
        Status = BillStatus.Settled;
        SettledAt = DateTime.UtcNow;

        _events.Add(new BillSettledEvent(BillNo, OrderNo, TableId, TableNo,
            Total, method.ToString()));
    }

    public void Void()
    {
        Status = BillStatus.Voided;
    }

    private void Recalculate()
    {
        Subtotal = _items.Sum(i => i.Total);
        TaxableAmount = Subtotal - DiscountAmount;
        TaxAmount = Math.Round(TaxableAmount * (TaxRate / 100), 2);
        Total = TaxableAmount + TaxAmount;
    }

    public void ClearEvents() => _events.Clear();
}