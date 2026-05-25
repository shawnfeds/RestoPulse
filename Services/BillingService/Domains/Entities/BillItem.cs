namespace RestoPulse.BillingService.Domain.Entities;

public class BillItem
{
    public int Id { get; private set; }
    public int BillId { get; private set; }
    public int MenuItemId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Qty { get; private set; }
    public decimal Total => Price * Qty;

    private BillItem() { }

    public static BillItem Create(int menuItemId, string name, decimal price, int qty)
    {
        return new BillItem
        {
            MenuItemId = menuItemId,
            Name = name,
            Price = price,
            Qty = qty
        };
    }
}