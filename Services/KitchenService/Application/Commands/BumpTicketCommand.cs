using MediatR;

namespace RestoPulse.KitchenService.Application.Commands;

public record BumpTicketCommand(int Id) : IRequest<bool>;