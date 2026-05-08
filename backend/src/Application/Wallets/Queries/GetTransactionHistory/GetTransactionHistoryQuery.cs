using Application.Common.Models;
using Application.Common.Responses;
using Domain.Enums;
using MediatR;

namespace Application.Wallets.Queries.GetTransactionHistory;

public record GetTransactionHistoryQuery : IRequest<PaginatedList<TransactionHistoryResponse>>
{
    public long WalletId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public TransactionType? TransactionType { get; init; }
}