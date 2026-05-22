using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IAiService
{
    Task<string?> CategorizeTransactionAsync(string? description, string type, decimal amount, CancellationToken ct);
    Task<string> AnswerQueryAsync(string query, string currency, List<TransactionInfo> transactions, CancellationToken ct);
}
