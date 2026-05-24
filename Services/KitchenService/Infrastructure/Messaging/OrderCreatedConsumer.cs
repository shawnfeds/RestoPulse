using MassTransit;
using RestoPulse.KitchenService.Domain.Entities;
using RestoPulse.KitchenService.Infrastructure.Persistence;
using RestoPulse.OrderService.Domain.Events;

namespace RestoPulse.KitchenService.Infrastructure.Messaging;

public class OrderCreatedConsumer(KitchenDbContext db) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var msg = context.Message;

        // One kitchen ticket per item in the order
        foreach (var item in msg.Items)
        {
            var ticket = KitchenTicket.Create(
                msg.OrderNo, msg.TableNo,
                item.MenuItemId, item.Name,
                item.Qty, item.Notes,
                category: "Main"); // you can enrich this from MenuService later

            db.Tickets.Add(ticket);
        }

        await db.SaveChangesAsync();
    }
}