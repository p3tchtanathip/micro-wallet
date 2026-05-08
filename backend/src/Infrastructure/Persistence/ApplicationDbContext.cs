using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionEntry> TransactionEntries => Set<TransactionEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email)
                .IsRequired();

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.Password)
                .IsRequired();

            entity.Property(e => e.Provider)
                .HasConversion<string>();

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()");
        });

        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired();
            
            entity.HasIndex(e => e.Name)
                .IsUnique();

            entity.HasData(
                new Role { Id = 1, Name = RoleNames.Admin },
                new Role { Id = 2, Name = RoleNames.User }
            );
        });

        // UserRole
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            
            entity.HasKey(e => new { e.UserId, e.RoleId }); // Composite Key

            entity.HasOne(e => e.User)
                .WithMany(e => e.UserRoles)
                .HasForeignKey(e => e.UserId);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId);
        });

        // Wallet
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.ToTable("wallets");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.WalletNumber)
                .IsRequired();

            entity.HasIndex(e => e.WalletNumber)
                .IsUnique();

            entity.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(e => e.Balance)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            entity.Property<uint>("xmin")
                .IsRowVersion();

            entity.HasOne(e => e.User)
                .WithMany(u => u.Wallets)
                .HasForeignKey(e => e.UserId);

            entity.HasIndex(e => e.UserId);
        });

        // Transaction
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("transactions");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.ReferenceNo)
                .IsRequired();

            entity.HasIndex(e => e.ReferenceNo)
                .IsUnique();

            entity.HasIndex(e => e.IdempotencyKey)
                .IsUnique();

            // Enum to string
            entity.Property(e => e.Type)
                .HasConversion<string>();

            entity.Property(e => e.Status)
                .HasConversion<string>();
        });

        // TransactionEntry
        modelBuilder.Entity<TransactionEntry>(entity =>
        {
            entity.ToTable("transaction_entries");

            entity.HasKey(e => new { e.TransactionId, e.WalletId }); // Composite Key

            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.HasOne(e => e.Transaction)
                .WithMany(t => t.TransactionEntries)
                .HasForeignKey(e => e.TransactionId);

            entity.HasOne(e => e.Wallet)
                .WithMany(w => w.TransactionEntries)
                .HasForeignKey(e => e.WalletId);

            entity.HasIndex(e => e.WalletId);
        });
    }
}