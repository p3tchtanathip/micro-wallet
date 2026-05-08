using Application.Common.Responses;
using MediatR;

namespace Application.Wallets.Commands.Withdraw;

public record class WithdrawCommand(
    long WalletId,
    decimal Amount
) : IRequest<TransactionResponse>;