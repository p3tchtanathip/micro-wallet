using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Responses;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Wallets.Queries.GetTransactionHistory;

public class GetTransactionHistoryQueryHandler : IRequestHandler<GetTransactionHistoryQuery, PaginatedList<TransactionHistoryResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IRequestContext _requestContext;

    public GetTransactionHistoryQueryHandler(IApplicationDbContext context, IRequestContext requestContext)
    {
        _context = context;
        _requestContext = requestContext;
    }
    
    public async Task<PaginatedList<TransactionHistoryResponse>> Handle(GetTransactionHistoryQuery request, CancellationToken ct)
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

        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => 
                t.TransactionEntries.Any(e => e.WalletId == request.WalletId) &&
                (!request.TransactionType.HasValue || t.Type == request.TransactionType)
            )
            .OrderByDescending(t => t.CreatedAt);

        var count = await query.CountAsync(ct);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new
            {
                t.ReferenceNo,
                t.Type,
                t.Status,
                t.Description,
                t.CreatedAt,

                Entry = t.TransactionEntries
                    .Where(e => e.WalletId == request.WalletId)
                    .Select(e => new
                    {
                        e.Amount
                    })
                    .First(),

                Counterparty = t.TransactionEntries
                    .Where(e => e.WalletId != request.WalletId)
                    .Select(e => new
                    {
                        e.WalletId,
                        e.Wallet.User.FullName
                    })
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var result = items.Select(t => new TransactionHistoryResponse(
            t.ReferenceNo,
            t.Type.ToString(),
            t.Status.ToString(),
            t.Entry.Amount,
            t.Description,
            t.CreatedAt,
            t.Counterparty?.FullName,
            t.Counterparty?.WalletId,
            t.Type == TransactionType.Transfer
                ? (t.Entry.Amount < 0 ? "Sent" : "Received")
                : "N/A"
        )).ToList();

        return new PaginatedList<TransactionHistoryResponse>(
            result,
            count,
            request.PageNumber,
            request.PageSize
        );
    }
}
