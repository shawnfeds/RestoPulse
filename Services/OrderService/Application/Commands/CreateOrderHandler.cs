using MassTransit;
using MediatR;
using RestoPulse.OrderService.Contracts;
using RestoPulse.OrderService.Domain.Entities;
using RestoPulse.OrderService.Domain.Events;
using RestoPulse.OrderService.Infrastructure.Persistence;

namespace RestoPulse.OrderService.Application.Commands;

public class CreateOrderHandler(OrderDbContext db, IPublishEndpoint bus)
    : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(
        CreateOrderCommand cmd, CancellationToken ct)
    {
        var order = Order.Create(cmd.TableId, cmd.TableNo, cmd.StaffName);
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        foreach (var evt in order.Events)
            await bus.Publish(evt, ct);
        order.ClearEvents();

        return new OrderResponse(
            order.Id, order.OrderNo, order.TableId, order.TableNo,
            order.Status.ToString(), order.StaffName,
            order.Subtotal, order.Tax, order.Total, order.CreatedAt, []);
    }
}