using Domain.Constants;

namespace Domain.Entities;

public class Wallet
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public required string WalletNumber { get; set; }
    public string Currency { get; set; } = Currencies.THB;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Relations
    public virtual User User { get; set; } = null!;
    public virtual ICollection<TransactionEntry> TransactionEntries { get; set; } = new List<TransactionEntry>();
}