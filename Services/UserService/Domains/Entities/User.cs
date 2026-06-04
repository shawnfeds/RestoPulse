using RestoPulse.UserService.Domain.Enums;

namespace RestoPulse.UserService.Domain.Entities;

public class User
{
    public int Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User() { }

    public static User Create(string username, string fullName, string passwordHash, UserRole role)
    {
        return new User
        {
            Username = username,
            FullName = fullName,
            PasswordHash = passwordHash,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }
}
