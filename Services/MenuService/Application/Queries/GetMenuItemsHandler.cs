using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.MenuService.Contracts;
using RestoPulse.MenuService.Infrastructure.Persistence;

namespace RestoPulse.MenuService.Application.Queries;

public class GetMenuItemsHandler(MenuDbContext db)
    : IRequestHandler<GetMenuItemsQuery, List<MenuItemResponse>>
{
    public async Task<List<MenuItemResponse>> Handle(
        GetMenuItemsQuery request, CancellationToken ct)
    {
        var query = db.MenuItems
            .Include(m => m.Category)
            .AsQueryable();

        if (request.CategoryId.HasValue)
            query = query.Where(m => m.CategoryId == request.CategoryId.Value);

        return await query
            .OrderBy(m => m.Category.DisplayOrder)
            .ThenBy(m => m.Name)
            .Select(m => new MenuItemResponse(
                m.Id, m.Name, m.Description, m.Price,
                m.CategoryId, m.Category.Name,
                m.IsAvailable, m.PreparationTime, m.TaxRate))
            .ToListAsync(ct);
    }
}