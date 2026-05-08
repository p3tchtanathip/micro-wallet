using Application.Common.Responses;
using MediatR;

namespace Application.Wallets.Commands.Deposit;

public record class DepositCommand(
    long WalletId,
    decimal Amount
) : IRequest<TransactionResponse>;