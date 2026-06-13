using MediatR;
using RestoPulse.InventoryService.Application.Commands;
using RestoPulse.InventoryService.Application.Queries;
using RestoPulse.InventoryService.Contracts;

namespace RestoPulse.InventoryService.Api.Endpoints;

public static class InventoryEndpoints
{
    public static RouteGroupBuilder MapInventoryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (bool? lowStockOnly, IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetInventoryQuery(lowStockOnly ?? false))))
            .WithName("GetInventory")
            .WithSummary("Get all inventory items, optionally filter low stock only");

        group.MapGet("/low-stock", async (IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetInventoryQuery(LowStockOnly: true))))
            .WithName("GetLowStock")
            .WithSummary("Get items below minimum threshold");

        group.MapPost("/", async (CreateInventoryItemRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateInventoryItemCommand(
                req.MenuItemId, req.Name, req.Unit,
                req.InitialStock, req.MinThreshold, req.CostPerUnit));
            return Results.Created($"/api/inventory/{result.Id}", result);
        })
        .WithName("CreateInventoryItem")
        .WithSummary("Create a new inventory item");

        group.MapPost("/{id:int}/adjust", async (int id, AdjustStockRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new AdjustStockCommand(
                id, req.Type, req.Quantity, req.Reason));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("AdjustStock")
        .WithSummary("Adjust stock — Addition, Deduction, or Correction");

        group.MapGet("/usage-report", async (int? month, int? year, IMediator mediator) =>
        {
            var now = DateTime.UtcNow;
            var m = Math.Clamp(month ?? now.Month, 1, 12);
            var y = year ?? now.Year;
            var result = await mediator.Send(new GetInventoryUsageQuery(m, y));
            return Results.Ok(result);
        })
        .WithName("GetInventoryUsageReport")
        .WithSummary("Get monthly inventory usage ranked by most consumed items");

        return group;
    }
}