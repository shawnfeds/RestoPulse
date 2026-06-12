using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoPulse.UserService.Contracts;
using RestoPulse.UserService.Infrastructure.Persistence;

namespace RestoPulse.UserService.Application.Queries;

public record GetUsersQuery : IRequest<List<UserResponse>>;

public class GetUsersHandler(UserDbContext db) : IRequestHandler<GetUsersQuery, List<UserResponse>>
{
    public async Task<List<UserResponse>> Handle(GetUsersQuery query, CancellationToken ct)
    {
        return await db.Users
            .OrderBy(u => u.Role)
            .ThenBy(u => u.FullName)
            .Select(u => new UserResponse(
                u.Id,
                u.Username,
                u.FullName,
                u.Role,
                u.IsActive,
                u.CreatedAt
            ))
            .ToListAsync(ct);
    }
}
