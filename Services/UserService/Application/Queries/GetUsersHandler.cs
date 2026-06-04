using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.UserService.Contracts;
using RestoPulse.UserService.Infrastructure.Persistence;

namespace RestoPulse.UserService.Application.Queries;

public class GetUsersHandler(UserDbContext db)
    : IRequestHandler<GetUsersQuery, List<UserResponse>>
{
    public async Task<List<UserResponse>> Handle(
        GetUsersQuery request, CancellationToken ct)
    {
        return await db.Users
            .OrderBy(u => u.FullName)
            .Select(u => new UserResponse(
                u.Id,
                u.Username,
                u.FullName,
                u.Role.ToString(),
                u.CreatedAt))
            .ToListAsync(ct);
    }
}
