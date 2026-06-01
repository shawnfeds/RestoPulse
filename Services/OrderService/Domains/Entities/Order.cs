using RestoPulse.OrderService.Domain.Enums;
using RestoPulse.Contracts;

namespace RestoPulse.OrderService.Domain.Entities;

public class Order
{
    public int Id { get; private set; }
    public string OrderNo { get; private set; } = string.Empty;
    public int TableId { get; private set; }
    public int TableNo { get; private set; }
    public OrderStatus Status { get; private set; }
    public string StaffName { get; private set; } = string.Empty;
    public decimal Subtotal { get; private set; }
    public decimal Tax { get; private set; }
    public decimal Total { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private readonly List<OrderIntegrationEvent> _events = new();
    public IReadOnlyList<OrderIntegrationEvent> Events => _events.AsReadOnly();

    private Order() { }

    public static Order Create(int tableId, int tableNo, string staffName)
    {
        var order = new Order
        {
            TableId = tableId,
            TableNo = tableNo,
            StaffName = staffName,
            Status = OrderStatus.New,
            CreatedAt = DateTime.UtcNow
        };
        order.OrderNo = $"ORD-{DateTime.UtcNow:yyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
        order._events.Add(new OrderCreatedEvent(order.OrderNo, tableId, tableNo, staffName));
        return order;
    }

    public void AddItem(int menuItemId, string name, decimal price, int qty, string? notes)
    {
        _items.Add(OrderItem.Create(menuItemId, name, price, qty, notes));
        Recalculate();
    }

    public void UpdateItem(int itemId, int qty, string? notes)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException("Item not found");
        item.Update(qty, notes);
        Recalculate();
    }

    public void RemoveItem(int itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new InvalidOperationException("Item not found");
        _items.Remove(item);
        Recalculate();
    }

    public void SetStatus(OrderStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
        _events.Add(new OrderStatusChangedEvent(OrderNo, TableId, TableNo, status.ToString()));
    }

    public void Void()
    {
        Status = OrderStatus.Void;
        UpdatedAt = DateTime.UtcNow;
        _events.Add(new OrderStatusChangedEvent(OrderNo, TableId, TableNo, OrderStatus.Void.ToString()));
    }

    private void Recalculate()
    {
        Subtotal = _items.Sum(i => i.Price * i.Qty);
        Tax = Math.Round(Subtotal * 0.18m, 2);
        Total = Subtotal + Tax;
    }

    public void ClearEvents() => _events.Clear();
}