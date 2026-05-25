using MediatR;
using RestoPulse.BillingService.Contracts;

namespace RestoPulse.BillingService.Application.Commands;

public record CreateBillCommand(
    string OrderNo,
    int TableId,
    int TableNo,
    List<CreateBillItemRequest> Items) : IRequest<BillResponse>;