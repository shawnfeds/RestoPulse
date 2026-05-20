namespace RestoPulse.OrderService.Domain.Entities;

public class OrderItem
{
    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public int MenuItemId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Qty { get; private set; }
    public string? Notes { get; private set; }
    public DateTime AddedAt { get; private set; }

    private OrderItem() { }

    public static OrderItem Create(int menuItemId, string name,
        decimal price, int qty, string? notes)
    {
        return new OrderItem
        {
            MenuItemId = menuItemId,
            Name = name,
            Price = price,
            Qty = qty,
            Notes = notes,
            AddedAt = DateTime.UtcNow
        };
    }

    public void Update(int qty, string? notes)
    {
        Qty = qty;
        Notes = notes;
    }
}