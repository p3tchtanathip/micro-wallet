using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Wallet> Wallets { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<TransactionEntry> TransactionEntries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
