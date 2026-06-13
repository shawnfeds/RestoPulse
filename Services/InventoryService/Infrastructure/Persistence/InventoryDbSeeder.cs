using Microsoft.EntityFrameworkCore;
using RestoPulse.InventoryService.Domain.Entities;
using RestoPulse.InventoryService.Domain.Enums;

namespace RestoPulse.InventoryService.Infrastructure.Persistence;

public static class InventoryDbSeeder
{
    public static async Task SeedAsync(InventoryDbContext db)
    {
        if (await db.InventoryItems.AnyAsync()) return;

        var items = new List<InventoryItem>
        {
            InventoryItem.Create(1,  "Spring Roll Wrappers",   "pcs",  500m, 100m,   2m),
            InventoryItem.Create(2,  "Baguette",               "pcs",   50m,  10m,  30m),
            InventoryItem.Create(3,  "Chicken Wings (Raw)",    "kg",    30m,  10m, 180m),
            InventoryItem.Create(4,  "Chicken Breasts",        "kg",    50m,  15m, 220m),
            InventoryItem.Create(5,  "Paneer (Cottage Cheese)","kg",    25m,   8m, 250m),
            InventoryItem.Create(6,  "Noodles (Raw)",          "kg",    40m,  10m,  50m),
            InventoryItem.Create(7,  "Salmon Fillets",         "pcs",   20m,   5m, 300m),
            InventoryItem.Create(8,  "Baking Chocolate",       "kg",    15m,   5m, 400m),
            InventoryItem.Create(9,  "Gulab Jamun Mix",        "kg",    10m,   3m, 150m),
            InventoryItem.Create(10, "Lemons",                 "pcs",  150m,  30m,   5m),
            InventoryItem.Create(11, "Mango Pulp",             "kg",    20m,   5m, 120m),
            InventoryItem.Create(12, "Coffee Beans",           "kg",    15m,   4m, 800m),
        };

        db.InventoryItems.AddRange(items);
        await db.SaveChangesAsync();

        // ── Seed 5 weeks of daily usage (stock deduction) records ───────────────
        var saved = await db.InventoryItems.ToListAsync();
        var now   = DateTime.UtcNow;

        var rows = new List<(int itemId, AdjustmentType type, decimal qty, DateTime at, string src, string reason)>();

        int GetId(string name) => saved.First(i => i.Name == name).Id;

        for (int dayBack = 35; dayBack >= 1; dayBack--)
        {
            var day        = now.Date.AddDays(-dayBack).AddHours(14);
            var dow        = day.DayOfWeek;
            bool isWeekend = dow is DayOfWeek.Saturday or DayOfWeek.Sunday;
            decimal m      = isWeekend ? 1.6m : 1.0m;

            // High-frequency items deducted every day
            rows.Add((GetId("Chicken Breasts"),         AdjustmentType.Deduction, Math.Round(2.5m * m, 2), day, "OrderDeduction", "Daily kitchen usage"));
            rows.Add((GetId("Chicken Wings (Raw)"),     AdjustmentType.Deduction, Math.Round(1.8m * m, 2), day, "OrderDeduction", "Daily kitchen usage"));
            rows.Add((GetId("Paneer (Cottage Cheese)"), AdjustmentType.Deduction, Math.Round(1.5m * m, 2), day, "OrderDeduction", "Daily kitchen usage"));
            rows.Add((GetId("Spring Roll Wrappers"),    AdjustmentType.Deduction, Math.Round(30m  * m, 0), day, "OrderDeduction", "Daily kitchen usage"));
            rows.Add((GetId("Lemons"),                  AdjustmentType.Deduction, Math.Round(8m   * m, 0), day, "OrderDeduction", "Daily kitchen usage"));

            // Mid-frequency items
            if (dayBack % 2 == 0)
            {
                rows.Add((GetId("Noodles (Raw)"),  AdjustmentType.Deduction, Math.Round(1.2m * m, 2), day, "OrderDeduction", "Daily kitchen usage"));
                rows.Add((GetId("Coffee Beans"),   AdjustmentType.Deduction, Math.Round(0.5m * m, 2), day, "OrderDeduction", "Barista usage"));
                rows.Add((GetId("Mango Pulp"),     AdjustmentType.Deduction, Math.Round(0.8m * m, 2), day, "OrderDeduction", "Dessert station usage"));
            }

            // Lower-frequency items
            if (dayBack % 3 == 0)
            {
                rows.Add((GetId("Salmon Fillets"),    AdjustmentType.Deduction, Math.Round(4m   * m, 0), day, "OrderDeduction", "Daily kitchen usage"));
                rows.Add((GetId("Baking Chocolate"),  AdjustmentType.Deduction, Math.Round(0.7m * m, 2), day, "OrderDeduction", "Baking usage"));
                rows.Add((GetId("Gulab Jamun Mix"),   AdjustmentType.Deduction, Math.Round(0.4m * m, 2), day, "OrderDeduction", "Dessert station usage"));
                rows.Add((GetId("Baguette"),          AdjustmentType.Deduction, Math.Round(6m   * m, 0), day, "OrderDeduction", "Daily bread usage"));
            }

            // Weekly restocking (every 7 days)
            if (dayBack % 7 == 0)
            {
                var restock = day.AddHours(-5);
                rows.Add((GetId("Chicken Breasts"),        AdjustmentType.Addition, 25m,  restock, "Manual", "Delivery restock"));
                rows.Add((GetId("Paneer (Cottage Cheese)"),AdjustmentType.Addition, 15m,  restock, "Manual", "Delivery restock"));
                rows.Add((GetId("Spring Roll Wrappers"),   AdjustmentType.Addition, 300m, restock, "Manual", "Delivery restock"));
                rows.Add((GetId("Lemons"),                 AdjustmentType.Addition, 120m, restock, "Manual", "Delivery restock"));
                rows.Add((GetId("Coffee Beans"),           AdjustmentType.Addition,   8m, restock, "Manual", "Delivery restock"));
                rows.Add((GetId("Mango Pulp"),             AdjustmentType.Addition,  12m, restock, "Manual", "Delivery restock"));
            }
        }

        // Insert via raw SQL to bypass domain aggregate and set historical CreatedAt
        foreach (var (itemId, type, qty, at, src, reason) in rows)
        {
            var typeStr = type.ToString();
            await db.Database.ExecuteSqlRawAsync(
                @"INSERT INTO StockAdjustments
                    (InventoryItemId, Type, Quantity, StockBefore, StockAfter, Source, Reason, ReferenceNo, CreatedAt)
                  VALUES
                    ({0},{1},{2},{3},{4},{5},{6},{7},{8})",
                itemId, typeStr, qty, 0m, 0m, src, reason, (object?)null, at);
        }
    }
}
