using MediatR;
using RestoPulse.TableService.Contracts;

namespace RestoPulse.TableService.Application.Queries;

public record GetTablesQuery(string? Status) : IRequest<List<TableResponse>>;