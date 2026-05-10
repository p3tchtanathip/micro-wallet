using Application.Common.Responses;
using MediatR;

namespace Application.Wallets.Commands.Create;

public record class CreateCommand(
    string Currency
) : IRequest<WalletResponse>;