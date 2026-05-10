using Application.Common.Responses;
using MediatR;

namespace Application.Wallets.Commands.Withdraw;

public record class WithdrawCommand(
    long WalletId,
    decimal Amount,
    string? Description = null
) : IRequest<TransactionResponse>;