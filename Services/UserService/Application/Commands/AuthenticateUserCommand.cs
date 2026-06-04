using MediatR;
using RestoPulse.UserService.Contracts;

namespace RestoPulse.UserService.Application.Commands;

public record AuthenticateUserCommand(
    string Username,
    string Password) : IRequest<LoginResponse?>;
