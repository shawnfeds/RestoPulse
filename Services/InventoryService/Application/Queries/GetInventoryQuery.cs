using MediatR;
using RestoPulse.InventoryService.Contracts;

namespace RestoPulse.InventoryService.Application.Queries;

public record GetInventoryQuery(bool LowStockOnly = false) : IRequest<List<InventoryItemResponse>>;