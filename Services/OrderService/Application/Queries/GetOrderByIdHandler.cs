using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.OrderService.Contracts;
using RestoPulse.OrderService.Infrastructure.Persistence;

namespace RestoPulse.OrderService.Application.Queries;

public class GetOrderByIdHandler(OrderDbContext db)
    : IRequestHandler<GetOrderByIdQuery, OrderResponse?>
{
    public async Task<OrderResponse?> Handle(
        GetOrderByIdQuery request, CancellationToken ct)
    {
        var o = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, ct);

        if (o is null) return null;

        return new OrderResponse(
            o.Id, o.OrderNo, o.TableId, o.TableNo,
            o.Status.ToString(), o.StaffName,
            o.Subtotal, o.Tax, o.Total, o.CreatedAt,
            o.Items.Select(i => new OrderItemResponse(
                i.Id, i.MenuItemId, i.Name, i.Price, i.Qty, i.Notes))
            .ToList());
    }
}