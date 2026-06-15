using Microsoft.EntityFrameworkCore;
using RestoPulse.NotificationService.Domains.Entities;
using System.Threading.Tasks;

namespace RestoPulse.NotificationService.Infrastructure.Persistence;

public static class NotificationDbSeeder
{
    public static async Task SeedAsync(NotificationDbContext db)
    {
        if (await db.Notifications.AnyAsync()) return;

        // Seed typical notifications for today's date
        db.Notifications.Add(Notification.Create(
            "new_order",
            "New Order Placed",
            "ORD-MOCK-003 · Table 7 · 5 items",
            ["Chef", "Manager", "Owner"],
            "ORD-MOCK-003"
        ));

        db.Notifications.Add(Notification.Create(
            "dish_ready",
            "Dish Ready for Service",
            "Grilled Salmon · Table 7 · ORD-MOCK-003",
            ["Server", "Manager", "Owner"],
            "KT-102"
        ));

        db.Notifications.Add(Notification.Create(
            "bill_settled",
            "Bill Settled",
            "BILL-MOCK-101 · Table 2 · ₹1,227.20",
            ["Manager", "Owner"],
            "BILL-MOCK-101"
        ));

        await db.SaveChangesAsync();
    }
}
