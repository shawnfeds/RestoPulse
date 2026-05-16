using MediatR;
using RestoPulse.TableService.Contracts;
using RestoPulse.TableService.Domain.Entities;
using RestoPulse.TableService.Infrastructure.Persistence;

namespace RestoPulse.TableService.Application.Commands;

public class CreateTableHandler(TableDbContext db)
    : IRequestHandler<CreateTableCommand, TableResponse>
{
    public async Task<TableResponse> Handle(
        CreateTableCommand cmd, CancellationToken ct)
    {
        var table = Table.Create(cmd.TableNo, cmd.Capacity, cmd.Section);
        db.Tables.Add(table);
        await db.SaveChangesAsync(ct);

        return new TableResponse(table.Id, table.TableNo, table.Capacity,
            table.Section, table.Status.ToString(), null, null);
    }
}