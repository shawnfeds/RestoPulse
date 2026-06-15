using MassTransit;
using RestoPulse.Contracts;
using RestoPulse.NotificationService.Domains.Entities;
using RestoPulse.NotificationService.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace RestoPulse.NotificationService.Infrastructure.Messaging;

public class DishReadyConsumer(NotificationDbContext db) : IConsumer<DishReadyEvent>
{
    public async Task Consume(ConsumeContext<DishReadyEvent> context)
    {
        var msg = context.Message;

        var notification = Notification.Create(
            type: "dish_ready",
            title: "Dish Ready for Service",
            message: $"{msg.ItemName} · Table {msg.TableNo} · {msg.OrderNo}",
            forRoles: ["Server", "Manager", "Owner"],
            entityId: msg.TicketId.ToString()
        );

        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
    }
}
