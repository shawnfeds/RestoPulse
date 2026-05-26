using MediatR;
using RestoPulse.ReportService.Application.Queries;

namespace RestoPulse.ReportService.Api.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports").WithTags("Reports").RequireAuthorization();

        group.MapGet("/revenue", async (
            DateTime? from,
            DateTime? to,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;

            if (fromDate > toDate)
                return Results.BadRequest("'from' must be earlier than 'to'.");

            var result = await mediator.Send(new GetRevenueQuery(fromDate, toDate), ct);
            return Results.Ok(result);
        })
        .WithName("GetRevenue")
        .WithSummary("Get revenue report for a date range");

        group.MapGet("/top-items", async (
            DateTime? from,
            DateTime? to,
            int limit,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;
            var safeLimit = Math.Clamp(limit <= 0 ? 10 : limit, 1, 50);

            if (fromDate > toDate)
                return Results.BadRequest("'from' must be earlier than 'to'.");

            var result = await mediator.Send(new GetTopItemsQuery(fromDate, toDate, safeLimit), ct);
            return Results.Ok(result);
        })
        .WithName("GetTopItems")
        .WithSummary("Get top selling items for a date range");

        return app;
    }
}