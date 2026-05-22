using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Wallets.Commands.AiQuery;

public class AiQueryCommandHandler : IRequestHandler<AiQueryCommand, AiQueryResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AiQueryCommandHandler> _logger;
    private readonly IRequestContext _requestContext;
    private readonly IAiService _aiService;

    public AiQueryCommandHandler(
        IApplicationDbContext context,
        ILogger<AiQueryCommandHandler> logger,
        IRequestContext requestContext,
        IAiService aiService)
    {
        _context = context;
        _logger = logger;
        _requestContext = requestContext;
        _aiService = aiService;
    }

    public async Task<AiQueryResponse> Handle(AiQueryCommand request, CancellationToken ct)
    {
        if (_requestContext.UserId == null) throw new UnauthorizedAccessException();

        var currentUserId = long.Parse(_requestContext.UserId);

        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == request.WalletId, ct)
            ?? throw new NotFoundException("Wallet not found");

        if (!_requestContext.IsAdmin && wallet.UserId != currentUserId)
        {
            throw new ForbiddenAccessException();
        }

        var query = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.TransactionEntries.Any(e => e.WalletId == request.WalletId))
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Type,
                t.Category,
                t.Description,
                t.CreatedAt,

                Entry = t.TransactionEntries
                    .Where(e => e.WalletId == request.WalletId)
                    .Select(e => new
                    {
                        e.Amount
                    })
                    .First()
            })
            .ToListAsync(ct);

        var transactions = query.Select(t => new TransactionInfo(
            t.Type.ToString(),
            t.Category,
            t.Entry.Amount,
            t.Description,
            t.CreatedAt
        )).ToList();

        if (transactions.Count == 0)
        {
            _logger.LogInformation("No transactions found for wallet {WalletId}", request.WalletId);
            return new AiQueryResponse("No transactions available for analysis.");
        }

        _logger.LogInformation("AI query requested for wallet {WalletId} by user {UserId}", request.WalletId, currentUserId);

        var response = await _aiService.AnswerQueryAsync(request.Query, wallet.Currency, transactions, ct);

        return new AiQueryResponse(response);
    }
}