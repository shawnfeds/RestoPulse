using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestoPulse.UserService.Contracts;
using RestoPulse.UserService.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RestoPulse.UserService.Application.Commands;

public record LoginCommand(string Username, string Password) : IRequest<LoginResponse?>;

public class LoginHandler(UserDbContext db, IConfiguration configuration)
    : IRequestHandler<LoginCommand, LoginResponse?>
{
    public async Task<LoginResponse?> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var username = cmd.Username.ToLowerInvariant().Trim();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

        if (user == null || !user.IsActive)
            return null;

        var inputHash = HashPassword(cmd.Password);
        if (user.PasswordHash != inputHash)
            return null;

        var token = GenerateJwtToken(user);
        var userResponse = new UserResponse(
            user.Id,
            user.Username,
            user.FullName,
            user.Role,
            user.IsActive,
            user.CreatedAt
        );

        return new LoginResponse(token, userResponse);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private string GenerateJwtToken(Domain.Entities.User user)
    {
        var secret = configuration["Jwt:Secret"] ?? "SuperSecretKeyForRestoPulseUserService2026!";
        var issuer = configuration["Jwt:Issuer"] ?? "restopulse-user-service";
        var audience = configuration["Jwt:Audience"] ?? "restopulse-api";

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
