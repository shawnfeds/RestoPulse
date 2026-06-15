using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.NotificationService.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestoPulse.NotificationService.Application.Queries;

public record GetNotificationsQuery(string Role) : IRequest<List<NotificationDto>>;

public record NotificationDto(
    int Id,
    string Type,
    string Title,
    string Message,
    DateTime Timestamp,
    bool Read,
    string[] ForRoles,
    string? EntityId);

public class GetNotificationsHandler(NotificationDbContext db)
    : IRequestHandler<GetNotificationsQuery, List<NotificationDto>>
{
    public async Task<List<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var list = await db.Notifications
            .Where(n => n.Timestamp >= today)
            .OrderByDescending(n => n.Timestamp)
            .ToListAsync(cancellationToken);

        // Filter role in memory
        return list
            .Where(n => n.ForRoles.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(request.Role))
            .Select(n => new NotificationDto(
                n.Id,
                n.Type,
                n.Title,
                n.Message,
                n.Timestamp,
                n.Read,
                n.ForRoles.Split(',', StringSplitOptions.RemoveEmptyEntries),
                n.EntityId
            ))
            .ToList();
    }
}
