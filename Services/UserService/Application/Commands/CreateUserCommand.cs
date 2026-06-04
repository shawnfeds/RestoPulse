using MediatR;
using RestoPulse.UserService.Contracts;

namespace RestoPulse.UserService.Application.Commands;

public record CreateUserCommand(
    string Username,
    string FullName,
    string Password,
    string Role) : IRequest<UserResponse>;
