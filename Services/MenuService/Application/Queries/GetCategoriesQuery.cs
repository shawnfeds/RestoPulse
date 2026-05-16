using MediatR;
using RestoPulse.MenuService.Contracts;

namespace RestoPulse.MenuService.Application.Queries;

public record GetCategoriesQuery : IRequest<List<CategoryResponse>>;