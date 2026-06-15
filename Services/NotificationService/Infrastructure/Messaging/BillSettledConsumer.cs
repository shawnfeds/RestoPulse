using MassTransit;
using RestoPulse.Contracts;
using RestoPulse.NotificationService.Domains.Entities;
using RestoPulse.NotificationService.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace RestoPulse.NotificationService.Infrastructure.Messaging;

public class BillSettledConsumer(NotificationDbContext db) : IConsumer<BillSettledEvent>
{
    public async Task Consume(ConsumeContext<BillSettledEvent> context)
    {
        var msg = context.Message;

        var notification = Notification.Create(
            type: "bill_settled",
            title: "Bill Settled",
            message: $"{msg.BillNo} · Table {msg.TableNo} · ₹{msg.Total:F2}",
            forRoles: ["Manager", "Owner"],
            entityId: msg.BillNo
        );

        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
    }
}
