using MediatR;
using RestoPulse.MenuService.Infrastructure.Persistence;

namespace RestoPulse.MenuService.Application.Commands;

public class ToggleMenuItemHandler(MenuDbContext db)
    : IRequestHandler<ToggleMenuItemCommand, bool>
{
    public async Task<bool> Handle(
        ToggleMenuItemCommand cmd, CancellationToken ct)
    {
        var item = await db.MenuItems.FindAsync([cmd.Id], ct);
        if (item is null) return false;

        item.ToggleAvailability();
        await db.SaveChangesAsync(ct);
        return true;
    }
}