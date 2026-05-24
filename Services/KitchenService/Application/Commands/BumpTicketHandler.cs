using MediatR;
using RestoPulse.KitchenService.Infrastructure.Persistence;

namespace RestoPulse.KitchenService.Application.Commands;

public class BumpTicketHandler(KitchenDbContext db)
    : IRequestHandler<BumpTicketCommand, bool>
{
    public async Task<bool> Handle(BumpTicketCommand cmd, CancellationToken ct)
    {
        var ticket = await db.Tickets.FindAsync([cmd.Id], ct);
        if (ticket is null) return false;
        ticket.Bump();
        await db.SaveChangesAsync(ct);
        return true;
    }
}