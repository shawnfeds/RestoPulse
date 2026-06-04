using MediatR;
using RestoPulse.UserService.Contracts;

namespace RestoPulse.UserService.Application.Queries;

public record GetUsersQuery() : IRequest<List<UserResponse>>;
