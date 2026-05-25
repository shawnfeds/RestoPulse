using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.BillingService.Application.Queries;
using RestoPulse.BillingService.Contracts;
using RestoPulse.BillingService.Infrastructure.Persistence;

namespace RestoPulse.BillingService.Application.Commands;

public class ApplyDiscountHandler(BillingDbContext db)
    : IRequestHandler<ApplyDiscountCommand, BillResponse?>
{
    public async Task<BillResponse?> Handle(
        ApplyDiscountCommand cmd, CancellationToken ct)
    {
        var bill = await db.Bills
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == cmd.Id, ct);

        if (bill is null) return null;

        bill.ApplyDiscount(cmd.DiscountAmount);
        await db.SaveChangesAsync(ct);

        return GetBillsHandler.ToResponse(bill);
    }
}