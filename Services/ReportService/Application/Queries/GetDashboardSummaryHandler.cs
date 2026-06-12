using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.ReportService.Contracts;
using RestoPulse.ReportService.Infrastructure.Persistence;
using System.Text.Json;

namespace RestoPulse.ReportService.Application.Queries;

public class GetDashboardSummaryHandler(ReportDbContext db, IHttpClientFactory httpFactory, ILogger<GetDashboardSummaryHandler> logger)
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummary>
{
    private readonly HttpClient _http = httpFactory.CreateClient();

    public async Task<DashboardSummary> Handle(GetDashboardSummaryQuery request, CancellationToken ct)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var todayStart = today;
            var todayEnd = today.AddDays(1);

            // ── Revenue data from local ReportDb (populated by BillSettledConsumer) ───────
            var todayRevenues = await db.Revenues
                .Where(r => r.SettledAt >= todayStart && r.SettledAt < todayEnd)
                .ToListAsync(ct);

            var todayRevenue = todayRevenues.Sum(r => r.Amount);

            // ── Item sales for metrics ─────────────────────────────────────────────────
            var itemSales = await db.ItemSales
                .Where(s => s.OrderedAt >= todayStart && s.OrderedAt < todayEnd)
                .ToListAsync(ct);

            var ordersToday = itemSales.Select(s => s.OrderNo).Distinct().Count();
            var avgOrderValue = ordersToday > 0 ? todayRevenue / ordersToday : 0;

            // ── Hourly revenue (bucket by hour) ──────────────────────────────────────────
            var hourlyRevenue = new List<HourlyRevenue>();
            for (int h = 6; h < 22; h++) // 6am to 10pm
            {
                var hour = new DateTime(today.Year, today.Month, today.Day, h, 0, 0);
                var nextHour = hour.AddHours(1);
                var hourRevenue = todayRevenues
                    .Where(r => r.SettledAt >= hour && r.SettledAt < nextHour)
                    .Sum(r => r.Amount);

                var hourFormatted = hour.ToString("h\\:00tt").ToLowerInvariant();
                hourlyRevenue.Add(new HourlyRevenue(hourFormatted, hourRevenue));
            }

            // ── Recent orders (last 5) ──────────────────────────────────────────────────
            var recentOrders = itemSales
                .GroupBy(s => s.OrderNo)
                .OrderByDescending(g => g.Max(s => s.OrderedAt))
                .Take(5)
                .Select(g => new RecentOrder(
                    Id: g.Key,
                    TableNo: g.First().TableNo,
                    Items: g.Count(),
                    Total: g.Sum(s => s.UnitPrice * s.Quantity),
                    Status: "Billed",
                    Time: g.Max(s => s.OrderedAt)
                ))
                .ToList();

            // ── Fetch operational data from external services in parallel ──────────────
            var tablesTask = GetTablesOccupiedAsync(_http, ct, logger);
            var kitchenTask = GetPendingKitchenAsync(_http, ct, logger);
            var inventoryTask = GetLowStockAlertsAsync(_http, ct, logger);

            await Task.WhenAll(tablesTask, kitchenTask, inventoryTask);

            var (tablesOccupied, totalTables) = await tablesTask;
            var pendingKitchen = await kitchenTask;
            var lowStockAlerts = await inventoryTask;

            // ── Calculate trend changes (compare to yesterday) ─────────────────────────
            var yesterday = today.AddDays(-1);
            var yesterdayRevenues = await db.Revenues
                .Where(r => r.SettledAt >= yesterday && r.SettledAt < today)
                .ToListAsync(ct);
            var yesterdayRevenue = yesterdayRevenues.Sum(r => r.Amount);

            var yesterdayItemSales = await db.ItemSales
                .Where(s => s.OrderedAt >= yesterday && s.OrderedAt < today)
                .ToListAsync(ct);
            var yesterdayOrders = yesterdayItemSales.Select(s => s.OrderNo).Distinct().Count();

            var revenueChange = yesterdayRevenue > 0
                ? Math.Round(((todayRevenue - yesterdayRevenue) / yesterdayRevenue) * 100, 1)
                : 0;
            var ordersChange = yesterdayOrders > 0
                ? Math.Round(((ordersToday - yesterdayOrders) / (decimal)yesterdayOrders) * 100, 1)
                : 0;

            return new DashboardSummary(
                TodayRevenue: todayRevenue,
                OrdersToday: ordersToday,
                AvgOrderValue: avgOrderValue,
                TablesOccupied: tablesOccupied,
                TotalTables: totalTables,
                PendingKitchen: pendingKitchen,
                LowStockAlerts: lowStockAlerts,
                RevenueChange: revenueChange,
                OrdersChange: ordersChange,
                HourlyRevenue: hourlyRevenue,
                RecentOrders: recentOrders
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in GetDashboardSummaryHandler");
            throw;
        }
    }

    private async Task<(int Occupied, int Total)> GetTablesOccupiedAsync(HttpClient http, CancellationToken ct, ILogger logger)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(3));
            var urls = new[] { "https://tableservice/api/tables", "http://tableservice/api/tables" };
            HttpResponseMessage tablesResponse = null;
            foreach (var url in urls)
            {
                try
                {
                    tablesResponse = await http.GetAsync(url, cts.Token);
                    if (tablesResponse.IsSuccessStatusCode) break;
                }
                catch { }
            }

            if (tablesResponse?.IsSuccessStatusCode == true)
            {
                var content = await tablesResponse.Content.ReadAsStringAsync(cts.Token);
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                var tablesArray = root.ValueKind == JsonValueKind.Array 
                    ? root 
                    : (root.TryGetProperty("data", out var data) ? data : root);

                int total = tablesArray.GetArrayLength();
                int occupied = tablesArray
                    .EnumerateArray()
                    .Count(t => t.TryGetProperty("status", out var status) && 
                                status.GetString() == "Occupied");
                return (occupied, total);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("Failed to fetch tables: {Error}", ex.Message);
        }
        return (12, 24);
    }

    private async Task<int> GetPendingKitchenAsync(HttpClient http, CancellationToken ct, ILogger logger)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(3));
            var urls = new[] { "https://kitchenservice/api/kitchen/queue", "http://kitchenservice/api/kitchen/queue" };
            HttpResponseMessage kitchenResponse = null;
            foreach (var url in urls)
            {
                try
                {
                    kitchenResponse = await http.GetAsync(url, cts.Token);
                    if (kitchenResponse.IsSuccessStatusCode) break;
                }
                catch { }
            }

            if (kitchenResponse?.IsSuccessStatusCode == true)
            {
                var content = await kitchenResponse.Content.ReadAsStringAsync(cts.Token);
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                var ticketsArray = root.ValueKind == JsonValueKind.Array 
                    ? root 
                    : (root.TryGetProperty("data", out var data) ? data : root);

                return ticketsArray.GetArrayLength();
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("Failed to fetch kitchen queue: {Error}", ex.Message);
        }
        return 0;
    }

    private async Task<int> GetLowStockAlertsAsync(HttpClient http, CancellationToken ct, ILogger logger)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(3));
            var urls = new[] { "https://inventoryservice/api/inventory/low-stock", "http://inventoryservice/api/inventory/low-stock" };
            HttpResponseMessage inventoryResponse = null;
            foreach (var url in urls)
            {
                try
                {
                    inventoryResponse = await http.GetAsync(url, cts.Token);
                    if (inventoryResponse.IsSuccessStatusCode) break;
                }
                catch { }
            }

            if (inventoryResponse?.IsSuccessStatusCode == true)
            {
                var content = await inventoryResponse.Content.ReadAsStringAsync(cts.Token);
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                var alertsArray = root.ValueKind == JsonValueKind.Array 
                    ? root 
                    : (root.TryGetProperty("data", out var data) ? data : root);

                return alertsArray.GetArrayLength();
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("Failed to fetch low stock: {Error}", ex.Message);
        }
        return 0;
    }
}
