using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.NotificationService.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestoPulse.NotificationService.Application.Commands;

public record MarkAllNotificationsReadCommand(string Role) : IRequest<bool>;

public class MarkAllNotificationsReadHandler(NotificationDbContext db)
    : IRequestHandler<MarkAllNotificationsReadCommand, bool>
{
    public async Task<bool> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var list = await db.Notifications
            .Where(n => n.Timestamp >= today && !n.Read)
            .ToListAsync(cancellationToken);

        var roleNotifs = list
            .Where(n => n.ForRoles.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(request.Role));

        foreach (var notif in roleNotifs)
        {
            notif.MarkAsRead();
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
