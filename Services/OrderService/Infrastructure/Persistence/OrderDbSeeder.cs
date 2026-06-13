using Microsoft.EntityFrameworkCore;
using RestoPulse.OrderService.Domain.Entities;
using RestoPulse.OrderService.Domain.Enums;
using System.Reflection;

namespace RestoPulse.OrderService.Infrastructure.Persistence;

public static class OrderDbSeeder
{
    public static async Task SeedAsync(OrderDbContext db)
    {
        if (await db.Orders.AnyAsync()) return;

        // Order 1: Served order (Table 2)
        var order1 = Order.Create(2, 2, "Rahul Mehta (Server)");
        SetOrderNo(order1, "ORD-MOCK-001");
        order1.AddItem(1, "Spring Rolls", 150m, 2, "Extra spicy sauce");
        order1.AddItem(4, "Butter Chicken", 380m, 1, "Medium spice");
        order1.AddItem(11, "Mango Lassi", 110m, 2, null);
        order1.SetStatus(OrderStatus.Served);

        // Order 2: Preparing order (Table 5)
        var order2 = Order.Create(5, 5, "Rahul Mehta (Server)");
        SetOrderNo(order2, "ORD-MOCK-002");
        order2.AddItem(5, "Paneer Tikka Masala", 320m, 1, null);
        order2.AddItem(6, "Veg Hakka Noodles", 240m, 1, "Less oil");
        order2.AddItem(10, "Fresh Lime Soda", 80m, 2, "Sweet and salted");
        order2.SetStatus(OrderStatus.Preparing);

        // Order 3: Billed order (Table 7)
        var order3 = Order.Create(7, 7, "Rahul Mehta (Server)");
        SetOrderNo(order3, "ORD-MOCK-003");
        order3.AddItem(3, "Chicken Wings", 220m, 1, null);
        order3.AddItem(7, "Grilled Salmon", 490m, 2, "Well done");
        order3.AddItem(8, "Chocolate Lava Cake", 180m, 2, null);
        order3.AddItem(12, "Cold Brew Coffee", 140m, 2, "No sugar");
        order3.SetStatus(OrderStatus.Billed);

        // Order 4: New order (Table 3)
        var order4 = Order.Create(3, 3, "Rahul Mehta (Server)");
        SetOrderNo(order4, "ORD-MOCK-004");
        order4.AddItem(2, "Garlic Bread", 120m, 1, "With cheese");
        order4.AddItem(6, "Veg Hakka Noodles", 240m, 1, null);

        db.Orders.AddRange(order1, order2, order3, order4);
        await db.SaveChangesAsync();
    }

    private static void SetOrderNo(Order order, string orderNo)
    {
        var prop = typeof(Order).GetProperty(nameof(Order.OrderNo), BindingFlags.Public | BindingFlags.Instance);
        prop?.SetValue(order, orderNo);
    }
}
