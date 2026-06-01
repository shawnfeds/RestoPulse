namespace RestoPulse.ReportService.Contracts;

/// <summary>
/// Dashboard summary aggregating live metrics and recent orders.
/// Format matches frontend expectations from dashboard.js
/// </summary>
public record DashboardSummary(
    decimal TodayRevenue,
    int OrdersToday,
    decimal AvgOrderValue,
    int TablesOccupied,
    int TotalTables,
    int PendingKitchen,
    int LowStockAlerts,
    decimal RevenueChange,
    decimal OrdersChange,
    List<HourlyRevenue> HourlyRevenue,
    List<RecentOrder> RecentOrders
);

/// <summary>
/// Hourly revenue breakdown for chart visualization (6am to 10pm).
/// </summary>
public record HourlyRevenue(
    string Hour,
    decimal Revenue
);

/// <summary>
/// Recent order summary for dashboard table.
/// Status values: "Served", "Preparing", "Billed", "New", "Void"
/// </summary>
public record RecentOrder(
    string Id,
    int TableNo,
    int Items,
    decimal Total,
    string Status,
    DateTime Time
);
