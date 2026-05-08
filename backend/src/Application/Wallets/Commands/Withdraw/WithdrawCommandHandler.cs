using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Wallets.Commands.Withdraw;

public class WithdrawCommandHandler : IRequestHandler<WithdrawCommand, TransactionResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentGatewayService _gateway;
    private readonly IRequestContext _requestContext;

    public WithdrawCommandHandler(IApplicationDbContext context, IPaymentGatewayService gateway, IRequestContext requestContext)
    {
        _context = context;
        _gateway = gateway;
        _requestContext = requestContext;
    }

    public async Task<TransactionResponse> Handle(WithdrawCommand request, CancellationToken ct)
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
                var existingEntry = existingTransaction
                    .TransactionEntries
                    .FirstOrDefault();

                return new TransactionResponse(
                    existingTransaction.ReferenceNo,
                    existingTransaction.Status.ToString(),
                    existingEntry?.Amount ?? request.Amount,
                    wallet.Balance,
                    existingTransaction.CreatedAt
                );
            }
        }

        // Balance validation
        if (wallet.Balance < request.Amount)
        {
            throw new BadRequestException("Insufficient balance");
        }

        var transaction = new Transaction
        {
            ReferenceNo = Guid.NewGuid().ToString(),
            Type = TransactionType.Withdraw,
            Status = TransactionStatus.Pending,
            IdempotencyKey = idempotency,
            CreatedAt = DateTime.UtcNow,
        };

        _context.Transactions.Add(transaction);

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            var existingTransaction = await _context.Transactions
                .Include(t => t.TransactionEntries)
                .FirstAsync(
                    t => t.IdempotencyKey == idempotency,
                    ct);

            var existingEntry = existingTransaction
                .TransactionEntries
                .FirstOrDefault();

            return new TransactionResponse(
                existingTransaction.ReferenceNo,
                existingTransaction.Status.ToString(),
                existingEntry?.Amount ?? request.Amount,
                wallet.Balance,
                existingTransaction.CreatedAt
            );
        }

        var gatewayResult = await _gateway.WithdrawAsync(request.Amount);

        if (!gatewayResult.Success)
        {
            transaction.Status = TransactionStatus.Failed;

            await _context.SaveChangesAsync(ct);

            return new TransactionResponse(
                transaction.ReferenceNo,
                transaction.Status.ToString(),
                request.Amount,
                wallet.Balance,
                transaction.CreatedAt
            );
        }

        wallet.Balance -= request.Amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        transaction.Status = TransactionStatus.Success;

        _context.TransactionEntries.Add(new TransactionEntry
        {
            Transaction = transaction,
            Wallet = wallet,
            Amount = -request.Amount
        });

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "Wallet was modified by another transaction."
            );
        }

        return new TransactionResponse(
            transaction.ReferenceNo,
            transaction.Status.ToString(),
            request.Amount,
            wallet.Balance,
            transaction.CreatedAt
        );
    }
}
