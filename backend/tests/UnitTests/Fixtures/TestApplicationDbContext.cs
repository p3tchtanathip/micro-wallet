using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Fixtures;

public class TestApplicationDbContext : ApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Password).IsRequired();
            entity.Property(e => e.Provider).HasConversion<string>();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.ToTable("wallets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WalletNumber).IsRequired();
            entity.HasIndex(e => e.WalletNumber).IsUnique();
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.Balance).HasPrecision(18, 2).HasDefaultValue(0);
            entity.HasOne(e => e.User).WithMany(u => u.Wallets).HasForeignKey(e => e.UserId);
            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.HasOne(e => e.User).WithMany(u => u.UserRoles).HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Role).WithMany(r => r.UserRoles).HasForeignKey(e => e.RoleId);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("transactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReferenceNo).IsRequired();
            entity.HasIndex(e => e.ReferenceNo).IsUnique();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
        });

        modelBuilder.Entity<TransactionEntry>(entity =>
        {
            entity.ToTable("transaction_entries");
            entity.HasKey(e => new { e.TransactionId, e.WalletId });
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.HasOne(e => e.Transaction).WithMany(t => t.TransactionEntries).HasForeignKey(e => e.TransactionId);
            entity.HasOne(e => e.Wallet).WithMany(w => w.TransactionEntries).HasForeignKey(e => e.WalletId);
        });
    }
}