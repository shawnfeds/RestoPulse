using RestoPulse.KitchenService.Domain.Enums;

namespace RestoPulse.KitchenService.Domain.Entities;

public class KitchenTicket
{
    public int Id { get; private set; }
    public string TicketNo { get; private set; } = string.Empty;
    public string OrderNo { get; private set; } = string.Empty;
    public int TableNo { get; private set; }
    public int MenuItemId { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public int Qty { get; private set; }
    public string? Notes { get; private set; }
    public TicketStatus Status { get; private set; }
    public TicketPriority Priority { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public DateTime OrderedAt { get; private set; }
    public DateTime? PrepStartedAt { get; private set; }
    public DateTime? ReadyAt { get; private set; }
    public DateTime? BumpedAt { get; private set; }

    private KitchenTicket() { }

    public static KitchenTicket Create(string orderNo, int tableNo,
        int menuItemId, string itemName, int qty,
        string? notes, string category)
    {
        return new KitchenTicket
        {
            TicketNo = $"KT-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            OrderNo = orderNo,
            TableNo = tableNo,
            MenuItemId = menuItemId,
            ItemName = itemName,
            Qty = qty,
            Notes = notes,
            Category = category,
            Status = TicketStatus.Pending,
            Priority = TicketPriority.Normal,
            OrderedAt = DateTime.UtcNow
        };
    }

    public void StartPreparing()
    {
        Status = TicketStatus.Preparing;
        PrepStartedAt = DateTime.UtcNow;
    }

    public void MarkReady()
    {
        Status = TicketStatus.Ready;
        ReadyAt = DateTime.UtcNow;
    }

    public void Bump()
    {
        BumpedAt = DateTime.UtcNow;
    }

    public void SetRush() => Priority = TicketPriority.Rush;
}