using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.MenuService.Contracts;
using RestoPulse.MenuService.Infrastructure.Persistence;

namespace RestoPulse.MenuService.Application.Commands;

public class UpdateMenuItemHandler(MenuDbContext db)
    : IRequestHandler<UpdateMenuItemCommand, MenuItemResponse?>
{
    public async Task<MenuItemResponse?> Handle(
        UpdateMenuItemCommand cmd, CancellationToken ct)
    {
        var item = await db.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == cmd.Id, ct);

        if (item is null) return null;

        item.Update(cmd.Name, cmd.Description, cmd.Price,
            cmd.CategoryId, cmd.PreparationTime, cmd.TaxRate);

        await db.SaveChangesAsync(ct);

        return new MenuItemResponse(
            item.Id, item.Name, item.Description, item.Price,
            item.CategoryId, item.Category.Name,
            item.IsAvailable, item.PreparationTime, item.TaxRate);
    }
}