using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.ReportService.Infrastructure.Persistence;

namespace RestoPulse.ReportService.Application.Queries;

public class GetRevenueQueryHandler(ReportDbContext db)
    : IRequestHandler<GetRevenueQuery, RevenueReportDto>
{
    public async Task<RevenueReportDto> Handle(GetRevenueQuery request, CancellationToken ct)
    {
        var records = await db.Revenues
            .Where(r => r.SettledAt >= request.From && r.SettledAt <= request.To)
            .ToListAsync(ct);

        var totalRevenue   = records.Sum(r => r.Amount);
        var totalOrders    = records.Count;
        var avgOrderValue  = totalOrders > 0 ? totalRevenue / totalOrders : 0;
        var netProfit      = Math.Round(totalRevenue * 0.40m, 2); // estimated 40% margin

        var daily = records
            .GroupBy(r => DateOnly.FromDateTime(r.SettledAt))
            .OrderBy(g => g.Key)
            .Select(g => new DailyRevenueDto(g.Key, g.Sum(r => r.Amount), g.Count()))
            .ToList();

        var paymentBreakdown = records
            .GroupBy(r => r.PaymentMethod)
            .OrderByDescending(g => g.Sum(r => r.Amount))
            .Select(g => new PaymentBreakdownDto(g.Key, g.Sum(r => r.Amount), g.Count()))
            .ToList();

        return new RevenueReportDto(totalRevenue, totalOrders, avgOrderValue, netProfit, daily, paymentBreakdown);
    }
}