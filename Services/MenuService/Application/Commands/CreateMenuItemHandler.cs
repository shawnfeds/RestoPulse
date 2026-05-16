using MediatR;
using RestoPulse.MenuService.Contracts;
using RestoPulse.MenuService.Domain.Entities;
using RestoPulse.MenuService.Infrastructure.Persistence;

namespace RestoPulse.MenuService.Application.Commands;

public class CreateMenuItemHandler(MenuDbContext db)
    : IRequestHandler<CreateMenuItemCommand, MenuItemResponse>
{
    public async Task<MenuItemResponse> Handle(
        CreateMenuItemCommand cmd, CancellationToken ct)
    {
        var item = MenuItem.Create(
            cmd.Name, cmd.Description, cmd.Price,
            cmd.CategoryId, cmd.PreparationTime, cmd.TaxRate);

        db.MenuItems.Add(item);
        await db.SaveChangesAsync(ct);

        var category = await db.Categories.FindAsync([item.CategoryId], ct);

        return new MenuItemResponse(
            item.Id, item.Name, item.Description, item.Price,
            item.CategoryId, category?.Name ?? string.Empty,
            item.IsAvailable, item.PreparationTime, item.TaxRate);
    }
}