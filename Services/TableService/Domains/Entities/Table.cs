using RestoPulse.TableService.Domain.Enums;
using RestoPulse.TableService.Domain.Events;

namespace RestoPulse.TableService.Domain.Entities;

public class Table
{
    public int Id { get; private set; }
    public int TableNo { get; private set; }
    public int Capacity { get; private set; }
    public string Section { get; private set; } = string.Empty;
    public TableStatus Status { get; private set; }
    public string? CurrentOrderId { get; private set; }
    public string? AssignedStaff { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<TableStatusChangedDomainEvent> _domainEvents = [];
    public IReadOnlyList<TableStatusChangedDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Table() { }

    public static Table Create(int tableNo, int capacity, string section)
    {
        return new Table
        {
            TableNo = tableNo,
            Capacity = capacity,
            Section = section,
            Status = TableStatus.Available,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(int tableNo, int capacity, string section)
    {
        TableNo = tableNo;
        Capacity = capacity;
        Section = section;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetStatus(TableStatus status, string? orderId = null, string? staff = null)
    {
        var previous = Status;
        Status = status;
        CurrentOrderId = orderId ?? CurrentOrderId;
        AssignedStaff = staff ?? AssignedStaff;
        UpdatedAt = DateTime.UtcNow;

        if (status == TableStatus.Available)
        {
            CurrentOrderId = null;
            AssignedStaff = null;
        }

        _domainEvents.Add(new TableStatusChangedDomainEvent(Id, TableNo, previous, status));
    }

    public void ClearEvents() => _domainEvents.Clear();
}