using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IAiService
{
    Task<string> CategorizeTransactionAsync(string? description, string type, decimal amount, CancellationToken ct);
    Task<string> GenerateSpendingInsightsAsync(Dictionary<string, decimal> categoryTotals, CancellationToken ct);
    Task<string> AnswerQueryAsync(string query, List<TransactionInfo> transactions, CancellationToken ct);
}
