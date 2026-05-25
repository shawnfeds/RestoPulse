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

        return group;
    }
}