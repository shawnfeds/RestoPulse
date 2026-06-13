using Microsoft.EntityFrameworkCore;
using RestoPulse.KitchenService.Domain.Entities;

namespace RestoPulse.KitchenService.Infrastructure.Persistence;

public static class KitchenDbSeeder
{
    public static async Task SeedAsync(KitchenDbContext db)
    {
        if (await db.Tickets.AnyAsync()) return;

        // Tickets for ORD-MOCK-002 (Preparing)
        var t1 = KitchenTicket.Create("ORD-MOCK-002", 5, 5, "Paneer Tikka Masala", 1, null, "Mains");
        t1.StartPreparing(); // Mark as Preparing
        
        var t2 = KitchenTicket.Create("ORD-MOCK-002", 5, 6, "Veg Hakka Noodles", 1, "Less oil", "Mains");
        t2.StartPreparing();

        var t3 = KitchenTicket.Create("ORD-MOCK-002", 5, 10, "Fresh Lime Soda", 2, "Sweet and salted", "Beverages");
        t3.StartPreparing();
        t3.MarkReady(); // Mark as Ready

        // Tickets for ORD-MOCK-004 (New / Pending)
        var t4 = KitchenTicket.Create("ORD-MOCK-004", 3, 2, "Garlic Bread", 1, "With cheese", "Appetizers");
        var t5 = KitchenTicket.Create("ORD-MOCK-004", 3, 6, "Veg Hakka Noodles", 1, null, "Mains");

        db.Tickets.AddRange(t1, t2, t3, t4, t5);
        await db.SaveChangesAsync();
    }
}
