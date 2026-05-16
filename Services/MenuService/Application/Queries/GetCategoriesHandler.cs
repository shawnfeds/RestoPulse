using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.MenuService.Contracts;
using RestoPulse.MenuService.Infrastructure.Persistence;

namespace RestoPulse.MenuService.Application.Queries;

public class GetCategoriesHandler(MenuDbContext db)
    : IRequestHandler<GetCategoriesQuery, List<CategoryResponse>>
{
    public async Task<List<CategoryResponse>> Handle(
        GetCategoriesQuery request, CancellationToken ct)
    {
        return await db.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryResponse(
                c.Id, c.Name, c.Description,
                c.DisplayOrder, c.IsActive,
                c.MenuItems.Count(m => m.IsAvailable)))
            .ToListAsync(ct);
    }
}