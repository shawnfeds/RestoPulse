using MediatR;
using RestoPulse.MenuService.Contracts;

namespace RestoPulse.MenuService.Application.Queries;

public record GetMenuItemsQuery(int? CategoryId) : IRequest<List<MenuItemResponse>>;