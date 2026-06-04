using MediatR;
using RestoPulse.UserService.Application.Commands;
using RestoPulse.UserService.Application.Queries;
using RestoPulse.UserService.Contracts;

namespace RestoPulse.UserService.Api.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/login", async (LoginRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new AuthenticateUserCommand(req.Username, req.Password));
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        })
        .WithName("Login")
        .WithSummary("Authenticate user credentials");

        group.MapGet("/", async (IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetUsersQuery())))
            .WithName("GetUsers")
            .WithSummary("Get all users");

        group.MapPost("/", async (CreateUserRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateUserCommand(
                req.Username, req.FullName, req.Password, req.Role));
            return Results.Created($"/api/users/{result.Id}", result);
        })
        .WithName("CreateUser")
        .WithSummary("Create a new user");

        return group;
    }
}
