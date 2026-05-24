namespace RestoPulse.KitchenService.Contracts;

public record KitchenTicketResponse(
    int Id,
    string TicketNo,
    string OrderNo,
    int TableNo,
    string ItemName,
    int Qty,
    string? Notes,
    string Status,
    string Priority,
    string Category,
    DateTime OrderedAt,
    DateTime? PrepStartedAt,
    DateTime? ReadyAt);

public record SetTicketStatusRequest(string Status);