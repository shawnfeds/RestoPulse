using MediatR;
using RestoPulse.KitchenService.Contracts;

namespace RestoPulse.KitchenService.Application.Queries;

public record GetKitchenQueueQuery(string? Status) : IRequest<List<KitchenTicketResponse>>;