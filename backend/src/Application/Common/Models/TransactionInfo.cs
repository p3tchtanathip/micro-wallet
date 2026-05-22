namespace Application.Common.Models;

public record TransactionInfo(
    string Type,
    string? Category,
    decimal Amount,
    string? Description,
    DateTime CreatedAt
);