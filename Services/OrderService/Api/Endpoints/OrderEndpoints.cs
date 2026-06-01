using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.OrderService.Application.Commands;
using RestoPulse.OrderService.Application.Queries;
using RestoPulse.OrderService.Contracts;
using RestoPulse.OrderService.Domain.Enums;
using RestoPulse.OrderService.Infrastructure.Persistence;

namespace RestoPulse.OrderService.Api.Endpoints;

public static class OrderEndpoints
{
    public static RouteGroupBuilder MapOrderEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (string? status, int? tableId, IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetOrdersQuery(status, tableId))))
            .WithName("GetOrders")
            .WithSummary("Get orders, optionally filtered by status or table");

        group.MapGet("/{id:int}", async (int id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOrderByIdQuery(id));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetOrderById")
        .WithSummary("Get a single order with items");

        group.MapPost("/", async (CreateOrderRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(
                new CreateOrderCommand(req.TableId, req.TableNo, req.StaffName));
            return Results.Created($"/api/orders/{result.Id}", result);
        })
        .WithName("CreateOrder")
        .WithSummary("Create a new order — publishes OrderCreated event");

        group.MapPost("/{id:int}/items", async (int id, AddOrderItemRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new AddOrderItemCommand(
                id, req.MenuItemId, req.Name, req.Price, req.Qty, req.Notes));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("AddOrderItem")
        .WithSummary("Add item to an existing order");

        group.MapPut("/{id:int}/items/{itemId:int}", async (
            int id, int itemId, UpdateOrderItemRequest req,
            RestoPulse.OrderService.Infrastructure.Persistence.OrderDbContext db) =>
        {
            var order = await db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return Results.NotFound();
            order.UpdateItem(itemId, req.Qty, req.Notes);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("UpdateOrderItem")
        .WithSummary("Update item qty or notes");

        group.MapDelete("/{id:int}/items/{itemId:int}", async (
            int id, int itemId,
            RestoPulse.OrderService.Infrastructure.Persistence.OrderDbContext db) =>
        {
            var order = await db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return Results.NotFound();
            order.RemoveItem(itemId);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("RemoveOrderItem")
        .WithSummary("Remove item from order");

        group.MapPatch("/{id:int}/status", async (int id, SetOrderStatusRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new SetOrderStatusCommand(id, req.Status));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("SetOrderStatus")
        .WithSummary("Update order status — publishes OrderStatusChanged event");

        group.MapPatch("/{id:int}/void", async (int id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SetOrderStatusCommand(id, "Void"));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("VoidOrder")
        .WithSummary("Void an order");

        group.MapGet("/summary", async (OrderDbContext db, CancellationToken ct) =>
        {
            var today = DateTime.UtcNow.Date;
            return Results.Ok(new
            {
                ActiveOrders = await db.Orders.CountAsync(o => o.Status == OrderStatus.New, ct),
                TodayOrders = await db.Orders.CountAsync(o => o.CreatedAt >= today, ct),
            });
        });

        return group;
    }
}