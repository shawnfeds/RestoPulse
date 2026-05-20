using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.OrderService.Contracts;
using RestoPulse.OrderService.Domain.Entities;
using RestoPulse.OrderService.Domain.Enums;
using RestoPulse.OrderService.Infrastructure.Persistence;

namespace RestoPulse.OrderService.Application.Queries;

public class GetOrdersHandler(OrderDbContext db)
    : IRequestHandler<GetOrdersQuery, List<OrderResponse>>
{
    public async Task<List<OrderResponse>> Handle(
        GetOrdersQuery request, CancellationToken ct)
    {
        var query = db.Orders.Include(o => o.Items).AsQueryable();

        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<OrderStatus>(request.Status, out var status))
            query = query.Where(o => o.Status == status);

        if (request.TableId.HasValue)
            query = query.Where(o => o.TableId == request.TableId.Value);

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => ToResponse(o))
            .ToListAsync(ct);
    }

    private static OrderResponse ToResponse(Order o) => new(
        o.Id, o.OrderNo, o.TableId, o.TableNo,
        o.Status.ToString(), o.StaffName,
        o.Subtotal, o.Tax, o.Total, o.CreatedAt,
        o.Items.Select(i => new OrderItemResponse(
            i.Id, i.MenuItemId, i.Name, i.Price, i.Qty, i.Notes))
        .ToList());
}