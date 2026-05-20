using MediatR;
using RestoPulse.OrderService.Contracts;

namespace RestoPulse.OrderService.Application.Commands;

public record CreateOrderCommand(
    int TableId, int TableNo, string StaffName) : IRequest<OrderResponse>;