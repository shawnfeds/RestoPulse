using MassTransit;
using RestoPulse.Contracts;
using RestoPulse.NotificationService.Domains.Entities;
using RestoPulse.NotificationService.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace RestoPulse.NotificationService.Infrastructure.Messaging;

public class OrderCreatedConsumer(NotificationDbContext db) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var msg = context.Message;
        var itemCount = msg.Items?.Count ?? 0;
        var itemSuffix = itemCount == 1 ? "" : "s";

        var notification = Notification.Create(
            type: "new_order",
            title: "New Order Placed",
            message: $"{msg.OrderNo} · Table {msg.TableNo} · {itemCount} item{itemSuffix}",
            forRoles: ["Chef", "Manager", "Owner"],
            entityId: msg.OrderNo
        );

        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
    }
}
