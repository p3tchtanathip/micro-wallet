using Domain.Enums;

namespace Domain.Entities;

public class Transaction
{
    public long Id { get; set; }
    public required string ReferenceNo { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // Relations
    public virtual ICollection<TransactionEntry> TransactionEntries { get; set; } = new List<TransactionEntry>();
}