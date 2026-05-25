using MediatR;
using RestoPulse.BillingService.Contracts;

namespace RestoPulse.BillingService.Application.Queries;

public record GetBillByIdQuery(int Id) : IRequest<BillResponse?>;