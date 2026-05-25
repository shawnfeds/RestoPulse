using MediatR;
using RestoPulse.BillingService.Contracts;

namespace RestoPulse.BillingService.Application.Queries;

public record GetBillsQuery(string? Status, string? OrderNo) : IRequest<List<BillResponse>>;