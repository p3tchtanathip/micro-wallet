using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Wallets.Commands.Transfer;

public class TransferCommandHandler : IRequestHandler<TransferCommand, TransactionResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IRequestContext _requestContext;

    public TransferCommandHandler(IApplicationDbContext context, IRequestContext requestContext)
    {
        _context = context;
        _requestContext = requestContext;
    }

    public async Task<TransactionResponse> Handle(TransferCommand request, CancellationToken ct)
    {
        if (_requestContext.UserId == null) throw new UnauthorizedAccessException();
        var currentUserId = long.Parse(_requestContext.UserId);

        var senderWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == request.FromWalletId, ct)
            ?? throw new NotFoundException("Sender wallet not found");

        var receiverWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == request.ToWalletId, ct)
            ?? throw new NotFoundException("Receiver wallet not found");

        if (!_requestContext.IsAdmin && senderWallet.UserId != currentUserId)
        {
            throw new ForbiddenAccessException();
        }
        
        // Idempotency check
        var idempotency = _requestContext.IdempotencyKey;
        if (!string.IsNullOrWhiteSpace(idempotency))
        {
             var existingTransaction = await _context.Transactions
                .Include(t => t.TransactionEntries)
                .FirstOrDefaultAsync(
                    t => t.IdempotencyKey == idempotency,
                    ct);

            if (existingTransaction is not null)
            {
                var senderEntry = existingTransaction
                    .TransactionEntries
                    .FirstOrDefault();

                return new TransactionResponse(
                    existingTransaction.ReferenceNo,
                    existingTransaction.Status.ToString(),
                    senderEntry?.Amount ?? request.Amount,
                    senderWallet.Balance,
                    existingTransaction.CreatedAt
                );
            }
        }

        // Balance validation
        if (senderWallet.Balance < request.Amount)
        {
            throw new BadRequestException("Insufficient balance");
        }

        using var dbTransaction = await _context.BeginTransactionAsync(ct);

        try
        {
            var transaction = new Transaction
            {
                ReferenceNo = Guid.NewGuid().ToString(),
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Success,
                Description = request.Description,
                IdempotencyKey = idempotency,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Transactions.Add(transaction);

            // Source
            senderWallet.Balance -= request.Amount;
            senderWallet.UpdatedAt = DateTime.UtcNow;

            _context.TransactionEntries.Add(new TransactionEntry
            {
                Transaction = transaction,
                Wallet = senderWallet,
                Amount = -request.Amount
            });

            // Destination
            receiverWallet.Balance += request.Amount;
            receiverWallet.UpdatedAt = DateTime.UtcNow;

            _context.TransactionEntries.Add(new TransactionEntry
            {
                Transaction = transaction,
                Wallet = receiverWallet,
                Amount = request.Amount
            });

            await _context.SaveChangesAsync(ct);
            await dbTransaction.CommitAsync(ct);

            return new TransactionResponse(
                transaction.ReferenceNo,
                transaction.Status.ToString(),
                request.Amount,
                senderWallet.Balance,
                transaction.CreatedAt
            );
        }
        catch (Exception)
        {
            await dbTransaction.RollbackAsync(ct);
            throw;
        }
    }
}