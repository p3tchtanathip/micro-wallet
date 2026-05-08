using Application.Common.Responses;
using MediatR;

namespace Application.Wallets.Queries.GetBalanceSummary;

public record GetBalanceSummaryQuery(long WalletId) : IRequest<BalanceSummaryResponse>;
