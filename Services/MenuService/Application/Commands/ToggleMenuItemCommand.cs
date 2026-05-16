using MediatR;

namespace RestoPulse.MenuService.Application.Commands;

public record ToggleMenuItemCommand(int Id) : IRequest<bool>;