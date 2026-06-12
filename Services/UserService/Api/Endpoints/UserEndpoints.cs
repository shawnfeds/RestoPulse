using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RestoPulse.UserService.Application.Commands;
using RestoPulse.UserService.Application.Queries;
using RestoPulse.UserService.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RestoPulse.UserService.Api.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
    {
        // ── 1. Anonymous Login ──────────────────────────────────────────────
        group.MapPost("/login", async (LoginRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new LoginCommand(req.Username, req.Password));
            return result is null ? Results.Json(new { message = "Invalid username or password" }, statusCode: 401) : Results.Ok(result);
        })
        .WithName("Login")
        .WithSummary("Authenticate user and return token");

        // ── 2. Staff Management (Admin only) ───────────────────────────────
        group.MapGet("/", async (HttpContext context, IMediator mediator) =>
        {
            var (userId, role) = GetUserFromHeader(context);
            if (userId == 0 || (role != "Owner" && role != "Manager"))
                return Results.Json(new { message = "Unauthorized" }, statusCode: 403);

            var users = await mediator.Send(new GetUsersQuery());
            return Results.Ok(users);
        })
        .WithName("GetUsers")
        .WithSummary("List all user accounts");

        group.MapPost("/", async (CreateUserRequest req, HttpContext context, IMediator mediator) =>
        {
            var (userId, role) = GetUserFromHeader(context);
            if (userId == 0 || (role != "Owner" && role != "Manager"))
                return Results.Json(new { message = "Unauthorized" }, statusCode: 403);

            var result = await mediator.Send(new CreateUserCommand(req.Username, req.Password, req.FullName, req.Role));
            return result is null ? Results.Conflict(new { message = "Username already exists" }) : Results.Created($"/api/users/{result.Id}", result);
        })
        .WithName("CreateUser")
        .WithSummary("Register a new user");

        group.MapPut("/{id:int}", async (int id, UpdateUserRequest req, HttpContext context, IMediator mediator) =>
        {
            var (userId, role) = GetUserFromHeader(context);
            if (userId == 0 || (role != "Owner" && role != "Manager"))
                return Results.Json(new { message = "Unauthorized" }, statusCode: 403);

            var result = await mediator.Send(new UpdateUserCommand(id, req.FullName, req.Role));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("UpdateUser")
        .WithSummary("Edit user details");

        group.MapPut("/{id:int}/status", async (int id, bool isActive, HttpContext context, IMediator mediator) =>
        {
            var (userId, role) = GetUserFromHeader(context);
            if (userId == 0 || (role != "Owner" && role != "Manager"))
                return Results.Json(new { message = "Unauthorized" }, statusCode: 403);

            var success = await mediator.Send(new ToggleUserStatusCommand(id, isActive));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithName("ToggleUserStatus")
        .WithSummary("Activate/Deactivate user");

        group.MapPut("/{id:int}/password", async (int id, ChangePasswordRequest req, HttpContext context, IMediator mediator) =>
        {
            var (callerId, role) = GetUserFromHeader(context);
            if (callerId == 0)
                return Results.Json(new { message = "Unauthorized" }, statusCode: 401);

            // Allow if admin or changing own password
            var isAdmin = role == "Owner" || role == "Manager";
            if (!isAdmin && callerId != id)
                return Results.Json(new { message = "Forbidden" }, statusCode: 403);

            var success = await mediator.Send(new ChangePasswordCommand(id, req.CurrentPassword, req.NewPassword, isAdmin));
            return success ? Results.NoContent() : Results.BadRequest(new { message = "Invalid current password" });
        })
        .WithName("ChangePassword")
        .WithSummary("Change user password");

        // ── 3. Clocking operations ──────────────────────────────────────────
        group.MapPost("/clock-in", async (string? notes, HttpContext context, IMediator mediator) =>
        {
            var (userId, _) = GetUserFromHeader(context);
            if (userId == 0) return Results.Json(new { message = "Unauthorized" }, statusCode: 401);

            var result = await mediator.Send(new ClockInCommand(userId, notes));
            return result is null ? Results.BadRequest(new { message = "Failed to clock in" }) : Results.Ok(result);
        })
        .WithName("ClockIn")
        .WithSummary("Clock in for shift");

        group.MapPost("/clock-out", async (HttpContext context, IMediator mediator) =>
        {
            var (userId, _) = GetUserFromHeader(context);
            if (userId == 0) return Results.Json(new { message = "Unauthorized" }, statusCode: 401);

            var result = await mediator.Send(new ClockOutCommand(userId));
            return result is null ? Results.BadRequest(new { message = "No active shift to clock out from" }) : Results.Ok(result);
        })
        .WithName("ClockOut")
        .WithSummary("Clock out of shift");

        group.MapGet("/clock-status", async (HttpContext context, IMediator mediator) =>
        {
            var (userId, _) = GetUserFromHeader(context);
            if (userId == 0) return Results.Json(new { message = "Unauthorized" }, statusCode: 401);

            var status = await mediator.Send(new GetClockStatusQuery(userId));
            return Results.Ok(status);
        })
        .WithName("GetClockStatus")
        .WithSummary("Get active shift info");

        // ── 4. Shift Types and Logs ─────────────────────────────────────────
        group.MapGet("/shift-types", async (HttpContext context, IMediator mediator) =>
        {
            var (userId, _) = GetUserFromHeader(context);
            if (userId == 0) return Results.Json(new { message = "Unauthorized" }, statusCode: 401);

            var list = await mediator.Send(new GetShiftTypesQuery());
            return Results.Ok(list);
        })
        .WithName("GetShiftTypes")
        .WithSummary("List all shift categories");

        group.MapGet("/shifts", async (int? filterUserId, string? date, HttpContext context, IMediator mediator) =>
        {
            var (callerId, role) = GetUserFromHeader(context);
            if (callerId == 0) return Results.Json(new { message = "Unauthorized" }, statusCode: 401);

            var isAdmin = role == "Owner" || role == "Manager";
            if (!isAdmin)
            {
                // Non-admins can only see their own shifts
                filterUserId = callerId;
            }

            DateOnly? parsedDate = null;
            if (!string.IsNullOrEmpty(date) && DateOnly.TryParse(date, out var d))
                parsedDate = d;

            var list = await mediator.Send(new GetShiftsQuery(filterUserId, parsedDate));
            return Results.Ok(list);
        })
        .WithName("GetShifts")
        .WithSummary("Get shift logs");

        // ── 5. Shift Scheduling ─────────────────────────────────────────────
        group.MapPost("/schedule", async (SetScheduleRequest req, HttpContext context, IMediator mediator) =>
        {
            var (userId, role) = GetUserFromHeader(context);
            if (userId == 0 || (role != "Owner" && role != "Manager"))
                return Results.Json(new { message = "Unauthorized" }, statusCode: 403);

            var success = await mediator.Send(new SetScheduleCommand(req.UserId, req.Date, req.ShiftTypeId));
            return success ? Results.Ok(new { message = "Scheduled successfully" }) : Results.BadRequest(new { message = "Failed to schedule shift" });
        })
        .WithName("SetSchedule")
        .WithSummary("Assign a shift to user");

        group.MapGet("/schedules", async (string date, HttpContext context, IMediator mediator) =>
        {
            var (userId, role) = GetUserFromHeader(context);
            if (userId == 0 || (role != "Owner" && role != "Manager"))
                return Results.Json(new { message = "Unauthorized" }, statusCode: 403);

            if (!DateOnly.TryParse(date, out var d))
                return Results.BadRequest(new { message = "Invalid date format" });

            var list = await mediator.Send(new GetUserSchedulesQuery(d));
            return Results.Ok(list);
        })
        .WithName("GetSchedules")
        .WithSummary("Get all scheduled shifts for date");

        // ── 6. Reports ──────────────────────────────────────────────────────
        group.MapGet("/reports/monthly", async (int targetUserId, int month, int year, HttpContext context, IMediator mediator) =>
        {
            var (callerId, role) = GetUserFromHeader(context);
            if (callerId == 0) return Results.Json(new { message = "Unauthorized" }, statusCode: 401);

            var isAdmin = role == "Owner" || role == "Manager";
            if (!isAdmin && callerId != targetUserId)
                return Results.Json(new { message = "Forbidden" }, statusCode: 403);

            var report = await mediator.Send(new GetMonthlyHoursReportQuery(targetUserId, month, year));
            return report is null ? Results.NotFound() : Results.Ok(report);
        })
        .WithName("GetMonthlyHoursReport")
        .WithSummary("Generate monthly hours and overtime summary");

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
