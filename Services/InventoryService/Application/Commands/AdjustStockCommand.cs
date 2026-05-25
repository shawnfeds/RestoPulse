using MediatR;
using RestoPulse.InventoryService.Contracts;

namespace RestoPulse.InventoryService.Application.Commands;

public record AdjustStockCommand(
    int Id, string Type,
    decimal Quantity, string? Reason,
    string Source = "Manual",
    string? ReferenceNo = null) : IRequest<InventoryItemResponse?>;