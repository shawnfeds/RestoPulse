namespace RestoPulse.ReportService.Domain.Entities;

public class ItemSale
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string OrderNo { get; private set; } = string.Empty;
    public int TableId { get; private set; }
    public int TableNo { get; private set; }
    public int MenuItemId { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public DateTime OrderedAt { get; private set; }

    private ItemSale() { }

    public static ItemSale Create(
        string orderNo, int tableId, int tableNo,
        int menuItemId, string itemName,
        int quantity, decimal unitPrice,
        DateTime orderedAt) => new()
        {
            OrderNo = orderNo,
            TableId = tableId,
            TableNo = tableNo,
            MenuItemId = menuItemId,
            ItemName = itemName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            OrderedAt = orderedAt
        };
}