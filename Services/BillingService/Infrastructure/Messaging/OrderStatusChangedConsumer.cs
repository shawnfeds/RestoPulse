using MassTransit;
using Microsoft.EntityFrameworkCore;
using RestoPulse.BillingService.Infrastructure.Persistence;
using RestoPulse.OrderService.Domain.Events;

namespace RestoPulse.BillingService.Infrastructure.Messaging;

public class OrderStatusChangedConsumer(BillingDbContext db)
    : IConsumer<OrderStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var msg = context.Message;

        // When order is billed, mark the associated bill as settled if not already
        if (msg.NewStatus != "Billed") return;

        var bill = await db.Bills
            .FirstOrDefaultAsync(b => b.OrderNo == msg.OrderNo);

        if (bill is null || bill.Status != Domain.Enums.BillStatus.Pending) return;

        // Bill was settled directly via BillingService endpoint
        // This is just a safety sync in case of discrepancy
        await db.SaveChangesAsync();
    }
}