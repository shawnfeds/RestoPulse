using Microsoft.EntityFrameworkCore;
using RestoPulse.BillingService.Domain.Entities;
using RestoPulse.BillingService.Domain.Enums;
using System.Reflection;

namespace RestoPulse.BillingService.Infrastructure.Persistence;

public static class BillingDbSeeder
{
    public static async Task SeedAsync(BillingDbContext db)
    {
        if (await db.Bills.AnyAsync()) return;

        // Bill for ORD-MOCK-003 (Billed)
        var bill1 = Bill.Create("ORD-MOCK-003", 7, 7, 18m);
        bill1.AddItem(3, "Chicken Wings", 220m, 1);
        bill1.AddItem(7, "Grilled Salmon", 490m, 2);
        bill1.AddItem(8, "Chocolate Lava Cake", 180m, 2);
        bill1.AddItem(12, "Cold Brew Coffee", 140m, 2);
        // It stays Pending, because the order is just Billed, not Settled yet!
        db.Bills.Add(bill1);

        // Historical settled bills for Reports & Revenue (to test Billing history & Dashboard)
        var bill2 = Bill.Create("ORD-MOCK-101", 2, 2, 18m);
        bill2.AddItem(1, "Spring Rolls", 150m, 2);
        bill2.AddItem(4, "Butter Chicken", 380m, 1);
        bill2.AddItem(11, "Mango Lassi", 110m, 2);
        bill2.Settle(PaymentMethod.Cash, 1000m);
        SetSettledAt(bill2, DateTime.UtcNow.AddDays(-1));
        db.Bills.Add(bill2);

        var bill3 = Bill.Create("ORD-MOCK-102", 4, 4, 18m);
        bill3.AddItem(2, "Garlic Bread", 120m, 2);
        bill3.AddItem(5, "Paneer Tikka Masala", 320m, 1);
        bill3.AddItem(10, "Fresh Lime Soda", 80m, 2);
        bill3.Settle(PaymentMethod.UPI, 800m);
        SetSettledAt(bill3, DateTime.UtcNow.AddDays(-1));
        db.Bills.Add(bill3);

        await db.SaveChangesAsync();
    }

    private static void SetSettledAt(Bill bill, DateTime settledAt)
    {
        var prop = typeof(Bill).GetProperty(nameof(Bill.SettledAt), BindingFlags.Public | BindingFlags.Instance);
        prop?.SetValue(bill, settledAt);
    }
}
