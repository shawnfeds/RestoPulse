using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.OrderService.Contracts;
using RestoPulse.OrderService.Domain.Enums;
using RestoPulse.OrderService.Infrastructure.Persistence;

namespace RestoPulse.OrderService.Application.Commands;

public class SetOrderStatusHandler(OrderDbContext db, IPublishEndpoint bus)
    : IRequestHandler<SetOrderStatusCommand, OrderResponse?>
{
    public async Task<OrderResponse?> Handle(
        SetOrderStatusCommand cmd, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == cmd.Id, ct);

        if (order is null) return null;

        if (!Enum.TryParse<OrderStatus>(cmd.Status, out var status))
            throw new ArgumentException($"Invalid status: {cmd.Status}");

        order.SetStatus(status);
        await db.SaveChangesAsync(ct);

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