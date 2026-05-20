using MediatR;
using RestoPulse.OrderService.Contracts;

namespace RestoPulse.OrderService.Application.Queries;

public record GetOrdersQuery(string? Status, int? TableId) : IRequest<List<OrderResponse>>;