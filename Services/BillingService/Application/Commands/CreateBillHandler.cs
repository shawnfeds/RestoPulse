using MediatR;
using RestoPulse.BillingService.Application.Queries;
using RestoPulse.BillingService.Contracts;
using RestoPulse.BillingService.Domain.Entities;
using RestoPulse.BillingService.Infrastructure.Persistence;

namespace RestoPulse.BillingService.Application.Commands;

public class CreateBillHandler(BillingDbContext db)
    : IRequestHandler<CreateBillCommand, BillResponse>
{
    public async Task<BillResponse> Handle(
        CreateBillCommand cmd, CancellationToken ct)
    {
        var bill = Bill.Create(cmd.OrderNo, cmd.TableId, cmd.TableNo);

        if (cmd.Items != null)
        {
            foreach (var item in cmd.Items)
                bill.AddItem(item.MenuItemId, item.Name, item.Price, item.Qty);
        }

        db.Bills.Add(bill);
        await db.SaveChangesAsync(ct);

        return GetBillsHandler.ToResponse(bill);
    }
}