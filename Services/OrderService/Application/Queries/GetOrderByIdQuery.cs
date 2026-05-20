using MediatR;
using RestoPulse.OrderService.Contracts;

namespace RestoPulse.OrderService.Application.Queries;

public record GetOrderByIdQuery(int Id) : IRequest<OrderResponse?>;