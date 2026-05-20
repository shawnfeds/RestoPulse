using MediatR;
using RestoPulse.OrderService.Contracts;

namespace RestoPulse.OrderService.Application.Commands;

public record SetOrderStatusCommand(int Id, string Status) : IRequest<OrderResponse?>;