using MediatR;
using RestoPulse.OrderService.Contracts;

namespace RestoPulse.OrderService.Application.Commands;

public record AddOrderItemCommand(
    int OrderId, int MenuItemId, string Name,
    decimal Price, int Qty, string? Notes) : IRequest<OrderResponse?>;