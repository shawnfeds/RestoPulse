using MediatR;
using RestoPulse.BillingService.Contracts;

namespace RestoPulse.BillingService.Application.Commands;

public record ApplyDiscountCommand(int Id, decimal DiscountAmount) : IRequest<BillResponse?>;