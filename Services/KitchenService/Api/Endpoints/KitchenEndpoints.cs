using MediatR;
using RestoPulse.KitchenService.Application.Commands;
using RestoPulse.KitchenService.Application.Queries;
using RestoPulse.KitchenService.Contracts;

namespace RestoPulse.KitchenService.Api.Endpoints;

public static class KitchenEndpoints
{
    public static RouteGroupBuilder MapKitchenEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/queue", async (string? status, IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetKitchenQueueQuery(status))))
            .WithName("GetKitchenQueue")
            .WithSummary("Get active kitchen queue");

        group.MapPatch("/items/{id:int}/status", async (int id, SetTicketStatusRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new SetTicketStatusCommand(id, req.Status));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("SetTicketStatus")
        .WithSummary("Set ticket to Preparing or Ready");

        group.MapPost("/items/{id:int}/bump", async (int id, IMediator mediator) =>
        {
            var found = await mediator.Send(new BumpTicketCommand(id));
            return found ? Results.NoContent() : Results.NotFound();
        })
        .WithName("BumpTicket")
        .WithSummary("Bump ticket off the KDS screen");

        return group;
    }
}