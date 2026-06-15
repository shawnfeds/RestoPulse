using MediatR;
using RestoPulse.NotificationService.Infrastructure.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace RestoPulse.NotificationService.Application.Commands;

public record MarkNotificationReadCommand(int Id) : IRequest<bool>;

public class MarkNotificationReadHandler(NotificationDbContext db)
    : IRequestHandler<MarkNotificationReadCommand, bool>
{
    public async Task<bool> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await db.Notifications.FindAsync([request.Id], cancellationToken);
        if (notification is null) return false;

        notification.MarkAsRead();
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
