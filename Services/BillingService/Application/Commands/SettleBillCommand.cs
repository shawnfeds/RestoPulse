using MediatR;
using RestoPulse.BillingService.Contracts;

namespace RestoPulse.BillingService.Application.Commands;

public record SettleBillCommand(
    int Id,
    string PaymentMethod,
    decimal? AmountTendered) : IRequest<BillResponse?>;