using Application.Common.Responses;
using MediatR;

namespace Application.Wallets.Queries.GetUserWallet;

public record GetUserWalletQuery : IRequest<WalletResponse[]>;
