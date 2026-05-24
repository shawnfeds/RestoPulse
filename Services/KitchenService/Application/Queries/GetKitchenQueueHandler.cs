using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.KitchenService.Contracts;
using RestoPulse.KitchenService.Domain.Enums;
using RestoPulse.KitchenService.Infrastructure.Persistence;

namespace RestoPulse.KitchenService.Application.Queries;

public class GetKitchenQueueHandler(KitchenDbContext db)
    : IRequestHandler<GetKitchenQueueQuery, List<KitchenTicketResponse>>
{
    public async Task<List<KitchenTicketResponse>> Handle(
        GetKitchenQueueQuery request, CancellationToken ct)
    {
        var query = db.Tickets
            .Where(t => t.BumpedAt == null)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<TicketStatus>(request.Status, out var status))
            query = query.Where(t => t.Status == status);

        return await query
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.OrderedAt)
            .Select(t => new KitchenTicketResponse(
                t.Id, t.TicketNo, t.OrderNo, t.TableNo,
                t.ItemName, t.Qty, t.Notes,
                t.Status.ToString(), t.Priority.ToString(),
                t.Category, t.OrderedAt, t.PrepStartedAt, t.ReadyAt))
            .ToListAsync(ct);
    }
}