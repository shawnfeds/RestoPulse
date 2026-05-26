using MediatR;

namespace RestoPulse.ReportService.Application.Queries;

public record GetRevenueQuery(DateTime From, DateTime To) : IRequest<RevenueReportDto>;

public record RevenueReportDto(
    decimal TotalRevenue,
    int TotalOrders,
    decimal AverageOrderValue,
    IReadOnlyList<DailyRevenueDto> DailyBreakdown);

public record DailyRevenueDto(DateOnly Date, decimal Revenue, int Orders);