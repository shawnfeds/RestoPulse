using MediatR;
using RestoPulse.InventoryService.Contracts;

namespace RestoPulse.InventoryService.Application.Commands;

public record CreateInventoryItemCommand(
    int MenuItemId, string Name, string Unit,
    decimal InitialStock, decimal MinThreshold,
    decimal CostPerUnit) : IRequest<InventoryItemResponse>;