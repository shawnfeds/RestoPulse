using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.BillingService.Contracts;
using RestoPulse.BillingService.Infrastructure.Persistence;

namespace RestoPulse.BillingService.Application.Queries;

public class GetBillByIdHandler(BillingDbContext db)
    : IRequestHandler<GetBillByIdQuery, BillResponse?>
{
    public async Task<BillResponse?> Handle(
        GetBillByIdQuery request, CancellationToken ct)
    {
        var b = await db.Bills
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == request.Id, ct);

        return b is null ? null : GetBillsHandler.ToResponse(b);
    }
}