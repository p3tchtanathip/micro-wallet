namespace Application.Common.Responses;

public record TransactionResponse(
    string ReferenceNo,
    string Status,
    decimal Amount,
    decimal BalanceAfter,
    DateTime CreatedAt
);
