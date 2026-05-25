using MediatR;
using RestoPulse.BillingService.Application.Commands;
using RestoPulse.BillingService.Application.Queries;
using RestoPulse.BillingService.Contracts;

namespace RestoPulse.BillingService.Api.Endpoints;

public static class BillingEndpoints
{
    public static RouteGroupBuilder MapBillingEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (string? status, string? orderNo, IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetBillsQuery(status, orderNo))))
            .WithName("GetBills")
            .WithSummary("Get all bills, optionally filtered by status or orderNo");

        group.MapGet("/{id:int}", async (int id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBillByIdQuery(id));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetBillById")
        .WithSummary("Get a single bill with line items");

        group.MapPost("/", async (CreateBillRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateBillCommand(
                req.OrderNo, req.TableId, req.TableNo, req.Items));
            return Results.Created($"/api/bills/{result.Id}", result);
        })
        .WithName("CreateBill")
        .WithSummary("Create a bill from an order");

        group.MapPost("/{id:int}/settle", async (int id, SettleBillRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new SettleBillCommand(
                id, req.PaymentMethod, req.AmountTendered));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("SettleBill")
        .WithSummary("Settle a bill — publishes BillSettled event");

        group.MapPost("/{id:int}/discount", async (int id, ApplyDiscountRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new ApplyDiscountCommand(id, req.DiscountAmount));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("ApplyDiscount")
        .WithSummary("Apply a flat discount to a bill");

        group.MapPost("/{id:int}/split", async (int id, SplitBillRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new SplitBillCommand(id, req.SplitBy));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("SplitBill")
        .WithSummary("Calculate split amount per person");

        return group;
    }
}