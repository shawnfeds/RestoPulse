using MediatR;
using RestoPulse.BillingService.Contracts;

namespace RestoPulse.BillingService.Application.Commands;

public record SplitBillCommand(int Id, int SplitBy) : IRequest<SplitBillResponse?>;