namespace Domain.Entities;

public class Wallet
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public required string WalletNumber { get; set; }
    public required string Currency { get; set; }
    public decimal Balance { get; set; }
    public required byte[] RowVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Relations
    public virtual User User { get; set; } = null!;
    public virtual ICollection<TransactionEntry> TransactionEntries { get; set; } = new List<TransactionEntry>();
}