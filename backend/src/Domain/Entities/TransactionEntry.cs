namespace Domain.Entities;

public class TransactionEntry
{
    public long TransactionId { get; set; }
    public long WalletId { get; set; }
    public decimal Amount { get; set; }

    // Relations
    public virtual Transaction Transaction { get; set; } = null!;
    public virtual Wallet Wallet { get; set; } = null!;
}