using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Wallets.Commands.Transfer;

public class TransferCommandHandler : IRequestHandler<TransferCommand, TransactionResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IRequestContext _requestContext;
    private readonly IExchangeRateService _exchangeRateService;

    public TransferCommandHandler(IApplicationDbContext context, IRequestContext requestContext, IExchangeRateService exchangeRateService)
    {
        _context = context;
        _requestContext = requestContext;
        _exchangeRateService = exchangeRateService;
    }

    public async Task<TransactionResponse> Handle(TransferCommand request, CancellationToken ct)
    {
        if (_requestContext.UserId == null) throw new UnauthorizedAccessException();
        var currentUserId = long.Parse(_requestContext.UserId);

        var senderWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.WalletNumber == request.FromWalletNumber, ct)
            ?? throw new NotFoundException("Sender wallet not found");

        var receiverWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.WalletNumber == request.ToWalletNumber, ct)
            ?? throw new NotFoundException("Receiver wallet not found");

        if (senderWallet.WalletNumber == receiverWallet.WalletNumber)
            throw new BadRequestException("Cannot transfer to the same wallet");

        if (!_requestContext.IsAdmin && senderWallet.UserId != currentUserId)
            throw new ForbiddenAccessException();

        var convertedAmount = await GetConvertedAmountAsync(
            request.Amount,
            senderWallet.Currency,
            receiverWallet.Currency,
            ct);

        // Idempotency check
        var idempotency = _requestContext.IdempotencyKey;
        if (!string.IsNullOrWhiteSpace(idempotency))
        {
            var existingTransaction = await _context.Transactions
               .Include(t => t.TransactionEntries)
               .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotency, ct);

            if (existingTransaction is not null)
            {
                var senderEntry = existingTransaction
                    .TransactionEntries
                    .FirstOrDefault(e => e.Amount < 0);

                return new TransactionResponse(
                    existingTransaction.ReferenceNo,
                    existingTransaction.Status.ToString(),
                    senderEntry?.Amount ?? -request.Amount,
                    senderWallet.Balance,
                    existingTransaction.CreatedAt
                );
            }
        }

        // Balance validation
        if (senderWallet.Balance < request.Amount)
            throw new BadRequestException("Insufficient balance");

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
            receiverWallet.Balance += convertedAmount;
            receiverWallet.UpdatedAt = DateTime.UtcNow;

            _context.TransactionEntries.Add(new TransactionEntry
            {
                Transaction = transaction,
                Wallet = receiverWallet,
                Amount = convertedAmount
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

    // Helper
    private async Task<decimal> GetConvertedAmountAsync(
        decimal amount,
        string fromCurrency,
        string toCurrency,
        CancellationToken ct)
    {
        if (fromCurrency == toCurrency) return amount;

        var rate = await _exchangeRateService.GetUsdToThbRateAsync(ct);

        return (fromCurrency, toCurrency) switch
        {
            (Currencies.USD, Currencies.THB) => Math.Round(amount * rate, 2),
            (Currencies.THB, Currencies.USD) => Math.Round(amount / rate, 2),
            _ => throw new BadRequestException($"Unsupported currency pair: {fromCurrency} → {toCurrency}")
        };
    }
}