using MediatR;
using RestoPulse.ReportService.Application.Queries;

namespace RestoPulse.ReportService.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/summary", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetDashboardSummaryQuery(), ct);
            return Results.Ok(result);
        })
        .WithName("GetDashboardSummary")
        .WithSummary("Get dashboard summary with live metrics, occupancy, and recent orders");

        return app;
    }
}
