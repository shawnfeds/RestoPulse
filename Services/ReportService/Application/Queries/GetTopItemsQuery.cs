using MediatR;

namespace RestoPulse.ReportService.Application.Queries;

public record GetTopItemsQuery(DateTime From, DateTime To, int Limit = 10)
    : IRequest<IReadOnlyList<TopItemDto>>;

public record TopItemDto(
    int MenuItemId,
    string ItemName,
    int TotalQuantity,
    decimal TotalRevenue);