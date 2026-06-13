using Microsoft.EntityFrameworkCore;
using RestoPulse.ReportService.Domain.Entities;

namespace RestoPulse.ReportService.Infrastructure.Persistence;

public static class ReportDbSeeder
{
    public static async Task SeedAsync(ReportDbContext db)
    {
        if (await db.Revenues.AnyAsync() || await db.ItemSales.AnyAsync()) return;

        var now = DateTime.UtcNow;

        // ── Menu item catalogue (matches MenuService seeded items) ─────────────
        var menuItems = new[]
        {
            (id: 1,  name: "Spring Rolls",          price: 150m, catId: 1),
            (id: 2,  name: "Garlic Bread",          price: 120m, catId: 1),
            (id: 3,  name: "Chicken Wings",         price: 280m, catId: 1),
            (id: 4,  name: "Butter Chicken",        price: 380m, catId: 2),
            (id: 5,  name: "Paneer Tikka Masala",   price: 320m, catId: 2),
            (id: 6,  name: "Grilled Salmon",        price: 490m, catId: 2),
            (id: 7,  name: "Pasta Arrabbiata",      price: 340m, catId: 2),
            (id: 8,  name: "Naan",                  price:  60m, catId: 3),
            (id: 9,  name: "Garlic Naan",           price:  80m, catId: 3),
            (id: 10, name: "Fresh Lime Soda",       price:  80m, catId: 5),
            (id: 11, name: "Mango Lassi",           price: 110m, catId: 5),
            (id: 12, name: "Cold Brew Coffee",      price: 140m, catId: 5),
        };

        var revenues = new List<Revenue>();
        var itemSales = new List<ItemSale>();
        var random = new Random(42); // seeded for determinism

        // ── Seed 30 days of historical data ────────────────────────────────────
        int billCounter = 1000;
        int orderCounter = 1000;

        for (int dayBack = 30; dayBack >= 1; dayBack--)
        {
            var date = now.Date.AddDays(-dayBack);
            var dow  = date.DayOfWeek;
            bool isWeekend = dow is DayOfWeek.Saturday or DayOfWeek.Sunday;

            // 12-20 orders on weekdays, 20-32 on weekends
            int orderCount = isWeekend
                ? random.Next(20, 32)
                : random.Next(12, 20);

            for (int o = 0; o < orderCount; o++)
            {
                // Spread orders between 11am and 10pm
                int hour   = random.Next(11, 22);
                int minute = random.Next(0, 60);
                var orderTime = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0, DateTimeKind.Utc);

                int tableId = random.Next(1, 9);
                string orderNo = $"ORD-{orderCounter:D6}";
                string billNo  = $"BILL-{billCounter:D6}";
                orderCounter++;
                billCounter++;

                // Pick 1-4 random menu items
                int itemCount = random.Next(1, 5);
                var picked = menuItems.OrderBy(_ => random.Next()).Take(itemCount).ToList();

                decimal orderTotal = 0m;
                foreach (var item in picked)
                {
                    int qty = random.Next(1, 4);
                    itemSales.Add(ItemSale.Create(
                        orderNo, tableId, tableId,
                        item.id, item.name,
                        qty, item.price, orderTime));
                    orderTotal += item.price * qty;
                }

                // Add tax (18% rounded)
                orderTotal = Math.Round(orderTotal * 1.18m, 2);

                string payMethod = o % 3 == 0 ? "Cash" : (o % 3 == 1 ? "Card" : "UPI");

                revenues.Add(Revenue.Create(
                    billNo, orderNo,
                    tableId, tableId,
                    orderTotal, payMethod,
                    orderTime.AddMinutes(random.Next(5, 30))));
            }
        }

        db.Revenues.AddRange(revenues);
        db.ItemSales.AddRange(itemSales);
        await db.SaveChangesAsync();
    }
}
