using MediatR;
using RestoPulse.MenuService.Contracts;

namespace RestoPulse.MenuService.Application.Commands;

public record UpdateMenuItemCommand(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int CategoryId,
    int PreparationTime,
    decimal TaxRate) : IRequest<MenuItemResponse?>;