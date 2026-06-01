using MassTransit;
using Microsoft.EntityFrameworkCore;
using RestoPulse.Contracts;
using RestoPulse.ReportService.Domain.Entities;
using RestoPulse.ReportService.Infrastructure.Persistence;

namespace RestoPulse.ReportService.Infrastructure.Messaging;

public class OrderCreatedConsumer(ReportDbContext db, ILogger<OrderCreatedConsumer> logger)
    : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var msg = context.Message;

        if (await db.ItemSales.AnyAsync(
                r => r.OrderNo == msg.OrderNo,
                context.CancellationToken))
        {
            logger.LogInformation(
                "ItemSaleRecords for OrderNo {OrderNo} already exist. Skipping.", msg.OrderNo);
            return;
        }

        if (msg.Items is null or { Count: 0 })
        {
            logger.LogWarning(
                "OrderCreatedEvent for OrderNo {OrderNo} arrived with no items. Skipping.", msg.OrderNo);
            return;
        }

        foreach (var item in msg.Items)
        {
            db.ItemSales.Add(ItemSale.Create(
                orderNo: msg.OrderNo,
                tableId: msg.TableId,
                tableNo: msg.TableNo,
                menuItemId: item.MenuItemId,
                itemName: item.Name,
                quantity: item.Qty,
                unitPrice: item.Price,
                orderedAt: msg.OccurredAt));
        }

        await db.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "Item sales recorded for OrderNo {OrderNo} — {Count} item(s)",
            msg.OrderNo, msg.Items.Count);
    }
}