using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.BillingService.Contracts;
using RestoPulse.BillingService.Domain.Entities;
using RestoPulse.BillingService.Domain.Enums;
using RestoPulse.BillingService.Infrastructure.Persistence;

namespace RestoPulse.BillingService.Application.Queries;

public class GetBillsHandler(BillingDbContext db)
    : IRequestHandler<GetBillsQuery, List<BillResponse>>
{
    public async Task<List<BillResponse>> Handle(
        GetBillsQuery request, CancellationToken ct)
    {
        var query = db.Bills.Include(b => b.Items).AsQueryable();

        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<BillStatus>(request.Status, out var status))
            query = query.Where(b => b.Status == status);

        if (!string.IsNullOrEmpty(request.OrderNo))
            query = query.Where(b => b.OrderNo == request.OrderNo);

        return await query
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => ToResponse(b))
            .ToListAsync(ct);
    }

    internal static BillResponse ToResponse(Bill b) => new(
        b.Id, b.BillNo, b.OrderNo, b.TableNo,
        b.Status.ToString(), b.Subtotal, b.DiscountAmount,
        b.TaxAmount, b.TaxRate, b.Total,
        b.PaymentMethod.HasValue ? b.PaymentMethod.Value.ToString() : null,
        b.AmountTendered, b.ChangeReturned,
        b.CreatedAt, b.SettledAt,
        b.Items.Select(i => new BillItemResponse(
            i.Id, i.Name, i.Price, i.Qty, i.Total)).ToList());
}