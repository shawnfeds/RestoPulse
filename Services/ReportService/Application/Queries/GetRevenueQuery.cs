using MediatR;

namespace RestoPulse.ReportService.Application.Queries;

public record GetRevenueQuery(DateTime From, DateTime To) : IRequest<RevenueReportDto>;

public record RevenueReportDto(
    decimal TotalRevenue,
    int TotalOrders,
    decimal AverageOrderValue,
    decimal NetProfit,
    IReadOnlyList<DailyRevenueDto> DailyBreakdown,
    IReadOnlyList<PaymentBreakdownDto> PaymentBreakdown);

public record DailyRevenueDto(DateOnly Date, decimal Revenue, int Orders);
public record PaymentBreakdownDto(string Method, decimal Amount, int Count);