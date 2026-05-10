using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Wallets.Queries.GetUserWallet;

public class GetUserWalletQueryHandler : IRequestHandler<GetUserWalletQuery, WalletResponse[]>
{
    private readonly IApplicationDbContext _context;
    private readonly IRequestContext _requestContext;

    public GetUserWalletQueryHandler(IApplicationDbContext context, IRequestContext requestContext)
    {
        _context = context;
        _requestContext = requestContext;
    }

    public async Task<WalletResponse[]> Handle(GetUserWalletQuery request, CancellationToken ct)
    {
        if (_requestContext.UserId == null) throw new UnauthorizedAccessException();
        var currentUserId = long.Parse(_requestContext.UserId);

        return await _context.Wallets
            .AsNoTracking()
            .Where(x => x.UserId == currentUserId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new WalletResponse(x.Id, x.WalletNumber, x.Balance, x.Currency))
            .ToArrayAsync(ct);
    }
}
