using MediatR;
using RestoPulse.MenuService.Application.Commands;
using RestoPulse.MenuService.Application.Queries;
using RestoPulse.MenuService.Contracts;

namespace RestoPulse.MenuService.Api.Endpoints;

public static class MenuItemEndpoints
{
    public static RouteGroupBuilder MapMenuItemEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (string? categoryId, IMediator mediator) =>
        {
            // Handle empty categoryId from query param (e.g., ?categoryId=)
            int? catId = null;
            if (!string.IsNullOrWhiteSpace(categoryId) && int.TryParse(categoryId, out var parsedId))
                catId = parsedId;

            var result = await mediator.Send(new GetMenuItemsQuery(catId));
            return Results.Ok(result);
        })
            .WithName("GetMenuItems")
            .WithSummary("Get menu items, optionally filtered by category");

        group.MapPost("/", async (CreateMenuItemRequest req, IMediator mediator) =>
        {
            var cmd = new CreateMenuItemCommand(
                req.Name, req.Description, req.Price,
                req.CategoryId, req.PreparationTime, req.TaxRate);
            var result = await mediator.Send(cmd);
            return Results.Created($"/api/menu/items/{result.Id}", result);
        })
        .WithName("CreateMenuItem")
        .WithSummary("Create a new menu item");

        group.MapPut("/{id:int}", async (int id, UpdateMenuItemRequest req, IMediator mediator) =>
        {
            var cmd = new UpdateMenuItemCommand(
                id, req.Name, req.Description, req.Price,
                req.CategoryId, req.PreparationTime, req.TaxRate);
            var result = await mediator.Send(cmd);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("UpdateMenuItem")
        .WithSummary("Update a menu item");

        group.MapPatch("/{id:int}/toggle", async (int id, IMediator mediator) =>
        {
            var found = await mediator.Send(new ToggleMenuItemCommand(id));
            return found ? Results.NoContent() : Results.NotFound();
        })
        .WithName("ToggleMenuItem")
        .WithSummary("Toggle menu item availability");

        return group;
    }
}