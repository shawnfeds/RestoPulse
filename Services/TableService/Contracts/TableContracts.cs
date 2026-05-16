namespace RestoPulse.TableService.Contracts;

public record TableResponse(
    int Id,
    int TableNo,
    int Capacity,
    string Section,
    string Status,
    string? CurrentOrderId,
    string? AssignedStaff);

public record CreateTableRequest(
    int TableNo,
    int Capacity,
    string Section);

public record UpdateTableRequest(
    int TableNo,
    int Capacity,
    string Section);

public record SetTableStatusRequest(
    string Status,
    string? OrderId,
    string? AssignedStaff);