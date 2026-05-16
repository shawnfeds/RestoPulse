using RestoPulse.TableService.Domain.Enums;

namespace RestoPulse.TableService.Domain.Events;

// Internal domain event (not sent to bus)
public record TableStatusChangedDomainEvent(
    int TableId,
    int TableNo,
    TableStatus PreviousStatus,
    TableStatus NewStatus,
    DateTime OccurredAt)
{
    public TableStatusChangedDomainEvent(
        int tableId, int tableNo,
        TableStatus previous, TableStatus newStatus)
        : this(tableId, tableNo, previous, newStatus, DateTime.UtcNow) { }
}

// Integration event — this goes on RabbitMQ bus
public record TableStatusChangedEvent(
    int TableId,
    int TableNo,
    string PreviousStatus,
    string NewStatus,
    DateTime OccurredAt);