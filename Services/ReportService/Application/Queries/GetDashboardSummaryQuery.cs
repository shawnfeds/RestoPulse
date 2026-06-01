using MediatR;
using RestoPulse.ReportService.Contracts;

namespace RestoPulse.ReportService.Application.Queries;

public record GetDashboardSummaryQuery : IRequest<DashboardSummary>;
