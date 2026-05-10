using Application.Common.Responses;
using MediatR;

namespace Application.Wallets.Commands.Transfer;

public record class TransferCommand(
    string FromWalletNumber,
    string ToWalletNumber,
    decimal Amount,
    string? Description = null
) : IRequest<TransactionResponse>;
