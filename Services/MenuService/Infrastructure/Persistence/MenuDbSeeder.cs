using Microsoft.EntityFrameworkCore;
using RestoPulse.MenuService.Domain.Entities;

namespace RestoPulse.MenuService.Infrastructure.Persistence;

public static class MenuDbSeeder
{
    public static async Task SeedAsync(MenuDbContext db)
    {
        if (await db.Categories.AnyAsync()) return;

        var apps = Category.Create("Appetizers", "Delicious starters to kick off your meal", 1);
        var mains = Category.Create("Mains", "Hearty and satisfying main courses", 2);
        var desserts = Category.Create("Desserts", "Sweet treats to end your dining experience", 3);
        var beverages = Category.Create("Beverages", "Refreshing cold and hot drinks", 4);

        db.Categories.AddRange(apps, mains, desserts, beverages);
        await db.SaveChangesAsync();

        var item1 = MenuItem.Create("Spring Rolls", "Crispy fried rolls filled with savory vegetables", 150m, apps.Id, 10, 5.00m);
        var item2 = MenuItem.Create("Garlic Bread", "Toasted baguette slices topped with garlic butter and herbs", 120m, apps.Id, 8, 5.00m);
        var item3 = MenuItem.Create("Chicken Wings", "Spicy buffalo wings served with ranch dip", 220m, apps.Id, 12, 5.00m);

        var item4 = MenuItem.Create("Butter Chicken", "Tender chicken cooked in a rich, creamy tomato gravy", 380m, mains.Id, 20, 18.00m);
        var item5 = MenuItem.Create("Paneer Tikka Masala", "Marinated cottage cheese cubes grilled and cooked in a spiced masala gravy", 320m, mains.Id, 18, 18.00m);
        var item6 = MenuItem.Create("Veg Hakka Noodles", "Stir-fried noodles with crisp vegetables and savory sauces", 240m, mains.Id, 15, 18.00m);
        var item7 = MenuItem.Create("Grilled Salmon", "Pan-seared salmon fillet served with lemon butter sauce and steamed veggies", 490m, mains.Id, 22, 18.00m);

        var item8 = MenuItem.Create("Chocolate Lava Cake", "Warm chocolate cake with a molten chocolate center, served with vanilla ice cream", 180m, desserts.Id, 15, 18.00m);
        var item9 = MenuItem.Create("Gulab Jamun", "Soft, syrup-soaked milk solid dumplings served warm", 90m, desserts.Id, 5, 5.00m);

        var item10 = MenuItem.Create("Fresh Lime Soda", "Refreshing lime juice mixed with soda and simple syrup", 80m, beverages.Id, 5, 18.00m);
        var item11 = MenuItem.Create("Mango Lassi", "Traditional yogurt-based drink flavored with sweet mango pulp", 110m, beverages.Id, 5, 5.00m);
        var item12 = MenuItem.Create("Cold Brew Coffee", "Smooth and bold cold-steeped coffee served over ice", 140m, beverages.Id, 5, 18.00m);

        db.MenuItems.AddRange(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10, item11, item12);
        await db.SaveChangesAsync();
    }
}
