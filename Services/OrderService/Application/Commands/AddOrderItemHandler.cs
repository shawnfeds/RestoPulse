using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.OrderService.Contracts;
using RestoPulse.OrderService.Infrastructure.Persistence;

namespace RestoPulse.OrderService.Application.Commands;

public class AddOrderItemHandler(OrderDbContext db, IPublishEndpoint bus)
    : IRequestHandler<AddOrderItemCommand, OrderResponse?>
{
    public async Task<OrderResponse?> Handle(
        AddOrderItemCommand cmd, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct);

        if (order is null) return null;

        order.AddItem(cmd.MenuItemId, cmd.Name, cmd.Price, cmd.Qty, cmd.Notes);
        await db.SaveChangesAsync(ct);

        // Publish updated OrderCreated so Kitchen gets new items
        foreach (var evt in order.Events)
            await bus.Publish(evt, ct);
        order.ClearEvents();

        return new OrderResponse(
            order.Id, order.OrderNo, order.TableId, order.TableNo,
            order.Status.ToString(), order.StaffName,
            order.Subtotal, order.Tax, order.Total, order.CreatedAt,
            order.Items.Select(i => new OrderItemResponse(
                i.Id, i.MenuItemId, i.Name, i.Price, i.Qty, i.Notes)).ToList());
    }
}