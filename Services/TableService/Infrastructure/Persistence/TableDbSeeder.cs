using Microsoft.EntityFrameworkCore;
using RestoPulse.TableService.Domain.Entities;

namespace RestoPulse.TableService.Infrastructure.Persistence;

public static class TableDbSeeder
{
    public static async Task SeedAsync(TableDbContext db)
    {
        if (await db.Tables.AnyAsync()) return;

        var tables = new List<Table>
        {
            Table.Create(1, 2, "Main Hall"),
            Table.Create(2, 4, "Main Hall"),
            Table.Create(3, 4, "Main Hall"),
            Table.Create(4, 6, "Main Hall"),
            Table.Create(5, 2, "Balcony"),
            Table.Create(6, 4, "Balcony"),
            Table.Create(7, 8, "VIP Lounge"),
            Table.Create(8, 2, "Bar"),
            Table.Create(9, 2, "Bar")
        };

        db.Tables.AddRange(tables);
        await db.SaveChangesAsync();
    }
}
