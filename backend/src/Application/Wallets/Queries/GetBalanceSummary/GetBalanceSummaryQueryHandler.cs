using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Wallets.Queries.GetBalanceSummary;

public class GetBalanceSummaryQueryHandler : IRequestHandler<GetBalanceSummaryQuery, BalanceSummaryResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IRequestContext _requestContext;
    private readonly IExchangeRateService _exchangeRateService;

    public GetBalanceSummaryQueryHandler(IApplicationDbContext context, IRequestContext requestContext, IExchangeRateService exchangeRateService)
    {
        _context = context;
        _requestContext = requestContext;
        _exchangeRateService = exchangeRateService;
    }

    public async Task<BalanceSummaryResponse> Handle(GetBalanceSummaryQuery request, CancellationToken ct)
    {
        if (_requestContext.UserId == null) throw new UnauthorizedAccessException();
        var currentUserId = long.Parse(_requestContext.UserId);

        var wallets = await _context.Wallets
            .AsNoTracking()
            .Where(w => w.UserId == currentUserId)
            .ToListAsync(ct);

        if (wallets.Count == 0) throw new NotFoundException("Wallets not found");

        var rate = await _exchangeRateService.GetUsdToThbRateAsync(ct);
        decimal totalThb = 0;

        foreach (var wallet in wallets)
        {
            switch (wallet.Currency)
            {
                case Currencies.THB:
                    totalThb += wallet.Balance;
                    break;

                case Currencies.USD:
                    totalThb += wallet.Balance * rate;
                    break;
            }
        }

        var latestUpdatedAt = wallets.Max(w => w.UpdatedAt);

        return new BalanceSummaryResponse(
            Math.Round(totalThb, 2),
            rate,
            latestUpdatedAt);
    }
}
