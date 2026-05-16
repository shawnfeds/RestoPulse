using MediatR;
using RestoPulse.TableService.Contracts;

namespace RestoPulse.TableService.Application.Commands;

public record SetTableStatusCommand(
    int Id, string Status,
    string? OrderId, string? AssignedStaff) : IRequest<TableResponse?>;