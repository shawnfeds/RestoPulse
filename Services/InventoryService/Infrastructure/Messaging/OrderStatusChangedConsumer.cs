using MassTransit;
using MediatR;
using RestoPulse.InventoryService.Application.Commands;
using RestoPulse.OrderService.Domain.Events;

namespace RestoPulse.InventoryService.Infrastructure.Messaging;

// When an order is marked Served, auto-deduct ingredients
public class OrderStatusChangedConsumer(ISender mediator)
    : IConsumer<OrderStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var msg = context.Message;
        if (msg.NewStatus != "Served") return;

        // In a real system you'd have a recipe/ingredient mapping table
        // For now this is the hook — extend with your business logic
        // Example: deduct 0.3kg chicken per Butter Chicken ordered
        await Task.CompletedTask;
    }
}