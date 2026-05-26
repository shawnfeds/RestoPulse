using MassTransit;
using Microsoft.EntityFrameworkCore;
using RestoPulse.BillingService.Domain.Events;
using RestoPulse.ReportService.Domain.Entities;
using RestoPulse.ReportService.Infrastructure.Persistence;

namespace RestoPulse.ReportService.Infrastructure.Messaging;

public class BillSettledConsumer(ReportDbContext db, ILogger<BillSettledConsumer> logger)
    : IConsumer<BillSettledEvent>
{
    public async Task Consume(ConsumeContext<BillSettledEvent> context)
    {
        var msg = context.Message;

        if (await db.Revenues.AnyAsync(
                r => r.BillNo == msg.BillNo,
                context.CancellationToken))
        {
            logger.LogInformation(
                "RevenueRecord for BillNo {BillNo} already exists. Skipping.", msg.BillNo);
            return;
        }

        var record = Revenue.Create(
            billNo: msg.BillNo,
            orderNo: msg.OrderNo,
            tableId: msg.TableId,
            tableNo: msg.TableNo,
            amount: msg.Total,
            paymentMethod: msg.PaymentMethod,
            settledAt: msg.OccurredAt);

        db.Revenues.Add(record);
        await db.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "Revenue recorded for BillNo {BillNo}, OrderNo {OrderNo}, Amount {Amount}",
            msg.BillNo, msg.OrderNo, msg.Total);
    }
}