using Application.Common.Responses;
using MediatR;

namespace Application.Wallets.Queries.GetBalanceSummary;

public record GetBalanceSummaryQuery : IRequest<BalanceSummaryResponse>;
