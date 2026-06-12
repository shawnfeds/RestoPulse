using MediatR;
using RestoPulse.UserService.Infrastructure.Persistence;
using System.Security.Cryptography;
using System.Text;

namespace RestoPulse.UserService.Application.Commands;

public record ChangePasswordCommand(int Id, string? CurrentPassword, string NewPassword, bool IsAdminAction) : IRequest<bool>;

public class ChangePasswordHandler(UserDbContext db) : IRequestHandler<ChangePasswordCommand, bool>
{
    public async Task<bool> Handle(ChangePasswordCommand cmd, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([cmd.Id], ct);
        if (user == null) return false;

        // If not admin action, we must validate current password
        if (!cmd.IsAdminAction)
        {
            if (string.IsNullOrEmpty(cmd.CurrentPassword))
                return false;

            var currentHash = HashPassword(cmd.CurrentPassword);
            if (user.PasswordHash != currentHash)
                return false; // Wrong current password
        }

        var newHash = HashPassword(cmd.NewPassword);
        user.UpdatePassword(newHash);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
