using MediatR;
using RestoPulse.TableService.Contracts;

namespace RestoPulse.TableService.Application.Commands;

public record CreateTableCommand(
    int TableNo, int Capacity, string Section) : IRequest<TableResponse>;