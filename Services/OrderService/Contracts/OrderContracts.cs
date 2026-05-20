namespace RestoPulse.OrderService.Contracts;

public record OrderItemResponse(
    int Id,
    int MenuItemId,
    string Name,
    decimal Price,
    int Qty,
    string? Notes);

public record OrderResponse(
    int Id,
    string OrderNo,
    int TableId,
    int TableNo,
    string Status,
    string StaffName,
    decimal Subtotal,
    decimal Tax,
    decimal Total,
    DateTime CreatedAt,
    List<OrderItemResponse> Items);

public record CreateOrderRequest(
    int TableId,
    int TableNo,
    string StaffName);

public record AddOrderItemRequest(
    int MenuItemId,
    string Name,
    decimal Price,
    int Qty,
    string? Notes);

public record UpdateOrderItemRequest(
    int Qty,
    string? Notes);

public record SetOrderStatusRequest(string Status);