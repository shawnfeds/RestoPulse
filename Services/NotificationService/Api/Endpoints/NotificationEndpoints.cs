using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RestoPulse.NotificationService.Application.Commands;
using RestoPulse.NotificationService.Application.Queries;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace RestoPulse.NotificationService.Api.Endpoints;

public static class NotificationEndpoints
{
    public static RouteGroupBuilder MapNotificationEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (HttpContext context, IMediator mediator) =>
        {
            var (userId, role) = GetUserFromHeader(context);
            if (userId == 0 || string.IsNullOrEmpty(role))
                return Results.Json(new { message = "Unauthorized" }, statusCode: 401);

            var list = await mediator.Send(new GetNotificationsQuery(role));
            return Results.Ok(list);
        })
        .WithName("GetNotifications")
        .WithSummary("Get today's notifications for user role");

        group.MapPost("/{id:int}/read", async (int id, HttpContext context, IMediator mediator) =>
        {
            var (userId, role) = GetUserFromHeader(context);
            if (userId == 0 || string.IsNullOrEmpty(role))
                return Results.Json(new { message = "Unauthorized" }, statusCode: 401);

            var success = await mediator.Send(new MarkNotificationReadCommand(id));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("MarkNotificationRead")
        .WithSummary("Mark notification as read");

        group.MapPost("/read-all", async (HttpContext context, IMediator mediator) =>
        {
            var (userId, role) = GetUserFromHeader(context);
            if (userId == 0 || string.IsNullOrEmpty(role))
                return Results.Json(new { message = "Unauthorized" }, statusCode: 401);

            await mediator.Send(new MarkAllNotificationsReadCommand(role));
            return Results.NoContent();
        })
        .WithName("MarkAllNotificationsRead")
        .WithSummary("Mark all today's notifications for role as read");

        return group;
    }

    private static (int UserId, string Role) GetUserFromHeader(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return (0, string.Empty);
        }

        try
        {
            var tokenStr = authHeader["Bearer ".Length..].Trim();
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenStr);

            var idClaim = jwt.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role)?.Value;

            if (int.TryParse(idClaim, out var uid))
            {
                return (uid, roleClaim ?? string.Empty);
            }
        }
        catch
        {
            // Ignore token read failures
        }

        return (0, string.Empty);
    }
}
