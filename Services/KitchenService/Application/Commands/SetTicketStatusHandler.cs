using MediatR;
using RestoPulse.KitchenService.Contracts;
using RestoPulse.KitchenService.Domain.Enums;
using RestoPulse.KitchenService.Infrastructure.Persistence;

namespace RestoPulse.KitchenService.Application.Commands;

public class SetTicketStatusHandler(KitchenDbContext db)
    : IRequestHandler<SetTicketStatusCommand, KitchenTicketResponse?>
{
    public async Task<KitchenTicketResponse?> Handle(
        SetTicketStatusCommand cmd, CancellationToken ct)
    {
        var ticket = await db.Tickets.FindAsync([cmd.Id], ct);
        if (ticket is null) return null;

        if (cmd.Status == TicketStatus.Preparing.ToString()) ticket.StartPreparing();
        else if (cmd.Status == TicketStatus.Ready.ToString()) ticket.MarkReady();

        await db.SaveChangesAsync(ct);

        return new KitchenTicketResponse(
            ticket.Id, ticket.TicketNo, ticket.OrderNo, ticket.TableNo,
            ticket.ItemName, ticket.Qty, ticket.Notes,
            ticket.Status.ToString(), ticket.Priority.ToString(),
            ticket.Category, ticket.OrderedAt, ticket.PrepStartedAt, ticket.ReadyAt);
    }
}