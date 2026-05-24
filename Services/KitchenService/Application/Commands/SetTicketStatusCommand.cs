using MediatR;
using RestoPulse.KitchenService.Contracts;

namespace RestoPulse.KitchenService.Application.Commands;

public record SetTicketStatusCommand(int Id, string Status) : IRequest<KitchenTicketResponse?>;