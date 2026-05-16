using MediatR;
using RestoPulse.TableService.Application.Commands;
using RestoPulse.TableService.Application.Queries;
using RestoPulse.TableService.Contracts;

namespace RestoPulse.TableService.Api.Endpoints;

public static class TableEndpoints
{
    public static RouteGroupBuilder MapTableEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (string? status, IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetTablesQuery(status))))
            .WithName("GetTables")
            .WithSummary("Get all tables, optionally filtered by status");

        group.MapPost("/", async (CreateTableRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(
                new CreateTableCommand(req.TableNo, req.Capacity, req.Section));
            return Results.Created($"/api/tables/{result.Id}", result);
        })
        .WithName("CreateTable")
        .WithSummary("Create a new table");

        group.MapPut("/{id:int}", async (int id, UpdateTableRequest req, IMediator mediator,
            RestoPulse.TableService.Infrastructure.Persistence.TableDbContext db) =>
        {
            var table = await db.Tables.FindAsync(id);
            if (table is null) return Results.NotFound();
            table.Update(req.TableNo, req.Capacity, req.Section);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("UpdateTable")
        .WithSummary("Update table details");

        group.MapPatch("/{id:int}/status", async (int id, SetTableStatusRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(
                new SetTableStatusCommand(id, req.Status, req.OrderId, req.AssignedStaff));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("SetTableStatus")
        .WithSummary("Update table status — publishes event to bus");

        group.MapDelete("/{id:int}", async (int id,
            RestoPulse.TableService.Infrastructure.Persistence.TableDbContext db) =>
        {
            var table = await db.Tables.FindAsync(id);
            if (table is null) return Results.NotFound();
            db.Tables.Remove(table);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteTable")
        .WithSummary("Delete a table");

        return group;
    }
}