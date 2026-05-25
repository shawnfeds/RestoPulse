using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.BillingService.Application.Queries;
using RestoPulse.BillingService.Contracts;
using RestoPulse.BillingService.Domain.Enums;
using RestoPulse.BillingService.Infrastructure.Persistence;

namespace RestoPulse.BillingService.Application.Commands;

public class SettleBillHandler(BillingDbContext db, IPublishEndpoint bus)
    : IRequestHandler<SettleBillCommand, BillResponse?>
{
    public async Task<BillResponse?> Handle(
        SettleBillCommand cmd, CancellationToken ct)
    {
        var bill = await db.Bills
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == cmd.Id, ct);

        if (bill is null) return null;

        if (!Enum.TryParse<PaymentMethod>(cmd.PaymentMethod, out var method))
            throw new ArgumentException($"Invalid payment method: {cmd.PaymentMethod}");

        bill.Settle(method, cmd.AmountTendered);
        await db.SaveChangesAsync(ct);

        foreach (var evt in bill.Events)
            await bus.Publish(evt, ct);
        bill.ClearEvents();

        return GetBillsHandler.ToResponse(bill);
    }
}