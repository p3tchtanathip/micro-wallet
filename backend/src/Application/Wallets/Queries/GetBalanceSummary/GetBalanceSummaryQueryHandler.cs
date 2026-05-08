using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Wallets.Queries.GetBalanceSummary;

public class GetBalanceSummaryQueryHandler : IRequestHandler<GetBalanceSummaryQuery, BalanceSummaryResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IRequestContext _requestContext;

    public GetBalanceSummaryQueryHandler(IApplicationDbContext context, IRequestContext requestContext)
    {
        _context = context;
        _requestContext = requestContext;
    }

    public async Task<BalanceSummaryResponse> Handle(GetBalanceSummaryQuery request,  CancellationToken ct)
    {
        if (_requestContext.UserId == null) throw new UnauthorizedAccessException();
        var currentUserId = long.Parse(_requestContext.UserId);

        var wallet = await _context.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == request.WalletId, ct)
            ?? throw new NotFoundException("Wallet not found");

        if (!_requestContext.IsAdmin && wallet.UserId != currentUserId)
        {
            throw new ForbiddenAccessException();
        }

        return new BalanceSummaryResponse(
            wallet.WalletNumber,
            wallet.Balance,
            wallet.Currency,
            wallet.UpdatedAt
        );
    }
}
