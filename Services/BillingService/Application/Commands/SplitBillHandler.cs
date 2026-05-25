using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.BillingService.Contracts;
using RestoPulse.BillingService.Infrastructure.Persistence;

namespace RestoPulse.BillingService.Application.Commands;

public class SplitBillHandler(BillingDbContext db)
    : IRequestHandler<SplitBillCommand, SplitBillResponse?>
{
    public async Task<SplitBillResponse?> Handle(
        SplitBillCommand cmd, CancellationToken ct)
    {
        var bill = await db.Bills.FindAsync([cmd.Id], ct);
        if (bill is null) return null;

        if (cmd.SplitBy < 2)
            throw new ArgumentException("Split must be between at least 2 people.");

        var amountPerPerson = Math.Round(bill.Total / cmd.SplitBy, 2);

        return new SplitBillResponse(
            bill.BillNo, bill.Total, cmd.SplitBy, amountPerPerson);
    }
}