namespace RestoPulse.BillingService.Contracts;

public record BillItemResponse(
    int Id,
    string Name,
    decimal Price,
    int Qty,
    decimal Total);

public record BillResponse(
    int Id,
    string BillNo,
    string OrderNo,
    int TableNo,
    string Status,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TaxRate,
    decimal Total,
    string? PaymentMethod,
    decimal? AmountTendered,
    decimal? ChangeReturned,
    DateTime CreatedAt,
    DateTime? SettledAt,
    List<BillItemResponse> Items);

public record CreateBillRequest(
    string OrderNo,
    int TableId,
    int TableNo,
    List<CreateBillItemRequest> Items);

public record CreateBillItemRequest(
    int MenuItemId,
    string Name,
    decimal Price,
    int Qty);

public record SettleBillRequest(
    string PaymentMethod,
    decimal? AmountTendered);

public record ApplyDiscountRequest(decimal DiscountAmount);

public record SplitBillRequest(int SplitBy);

public record SplitBillResponse(
    string BillNo,
    decimal Total,
    int SplitBy,
    decimal AmountPerPerson);