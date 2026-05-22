using Application.Common.Responses;
using MediatR;

namespace Application.Wallets.Commands.AiQuery;

public record class AiQueryCommand(
    long WalletId,
    string Query) : IRequest<AiQueryResponse>;