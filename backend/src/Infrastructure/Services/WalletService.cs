using Application.Common.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class WalletService : IWalletService
{
    private readonly IApplicationDbContext _context;

    public WalletService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateUniqueWalletNumberAsync()
    {
        string walletNumber;
        bool isDuplicate;
        var random = new Random();

        do
        {
            walletNumber = string.Concat(Enumerable.Range(0, 10).Select(_ => random.Next(10).ToString()));
            isDuplicate = await _context.Wallets.AnyAsync(w => w.WalletNumber == walletNumber);
        } while (isDuplicate);

        return walletNumber;
    }

    public async Task CreateDefaultWalletAsync(User user)
    {
        var walletNumber = await GenerateUniqueWalletNumberAsync();
        
        var wallet = new Wallet
        {
            User = user,
            WalletNumber = walletNumber,
            Currency = Currencies.THB,
            Balance = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Wallets.Add(wallet);
    }
}
