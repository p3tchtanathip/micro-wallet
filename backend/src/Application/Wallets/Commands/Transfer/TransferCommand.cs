using Application.Common.Responses;
using MediatR;

namespace Application.Wallets.Commands.Transfer;

public record class TransferCommand(
    long FromWalletId,
    long ToWalletId,
    decimal Amount,
    string? Description = null
) : IRequest<TransactionResponse>;
