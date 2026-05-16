using MediatR;
using RestoPulse.MenuService.Application.Queries;
using RestoPulse.MenuService.Contracts;
using RestoPulse.MenuService.Domain.Entities;
using RestoPulse.MenuService.Infrastructure.Persistence;

namespace RestoPulse.MenuService.Api.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetCategoriesQuery())))
            .WithName("GetCategories")
            .WithSummary("Get all active categories");

        group.MapPost("/", async (CreateCategoryRequest req, MenuDbContext db) =>
        {
            var cat = Category.Create(req.Name, req.Description, req.DisplayOrder);
            db.Categories.Add(cat);
            await db.SaveChangesAsync();
            return Results.Created($"/api/menu/categories/{cat.Id}",
                new CategoryResponse(cat.Id, cat.Name, cat.Description,
                    cat.DisplayOrder, cat.IsActive, 0));
        })
        .WithName("CreateCategory")
        .WithSummary("Create a new category");

        group.MapPut("/{id:int}", async (int id, UpdateCategoryRequest req, MenuDbContext db) =>
        {
            var cat = await db.Categories.FindAsync(id);
            if (cat is null) return Results.NotFound();
            cat.Update(req.Name, req.Description, req.DisplayOrder);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("UpdateCategory")
        .WithSummary("Update a category");

        return group;
    }
}