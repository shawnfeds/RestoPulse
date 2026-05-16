using MassTransit;
using MediatR;
using RestoPulse.TableService.Contracts;
using RestoPulse.TableService.Domain.Enums;
using RestoPulse.TableService.Domain.Events;
using RestoPulse.TableService.Infrastructure.Persistence;

namespace RestoPulse.TableService.Application.Commands;

public class SetTableStatusHandler(TableDbContext db, IPublishEndpoint bus)
    : IRequestHandler<SetTableStatusCommand, TableResponse?>
{
    public async Task<TableResponse?> Handle(
        SetTableStatusCommand cmd, CancellationToken ct)
    {
        var table = await db.Tables.FindAsync([cmd.Id], ct);
        if (table is null) return null;

        if (!Enum.TryParse<TableStatus>(cmd.Status, out var status))
            throw new ArgumentException($"Invalid status: {cmd.Status}");

        table.SetStatus(status, cmd.OrderId, cmd.AssignedStaff);
        await db.SaveChangesAsync(ct);

        // Publish integration event to RabbitMQ
        foreach (var evt in table.DomainEvents)
        {
            await bus.Publish(new TableStatusChangedEvent(
                evt.TableId,
                evt.TableNo,
                evt.PreviousStatus.ToString(),
                evt.NewStatus.ToString(),
                evt.OccurredAt), ct);
        }
        table.ClearEvents();

        return new TableResponse(table.Id, table.TableNo, table.Capacity,
            table.Section, table.Status.ToString(),
            table.CurrentOrderId, table.AssignedStaff);
    }
}