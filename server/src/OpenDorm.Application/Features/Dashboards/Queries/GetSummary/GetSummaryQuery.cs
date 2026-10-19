using MediatR;

namespace OpenDorm.Application.Features.Dashboards.Queries.GetSummary;

public record GetSummaryQuery : IRequest<SummaryDto>;